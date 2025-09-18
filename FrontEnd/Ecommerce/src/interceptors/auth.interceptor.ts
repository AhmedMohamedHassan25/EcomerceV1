import { inject } from '@angular/core';
import { HttpRequest, HttpHandlerFn, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth-service';
import { LoginResponse } from '../models/User'; // Import your LoginResponse type

// Global variables for token refresh state management
let isRefreshing = false;
let refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  // Only add token if user is authenticated and we have a valid token
  if (authService.isAuthenticated) {
    const token = authService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
  }

  return next(req).pipe(
    catchError(error => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        return handle401Error(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function handle401Error(
  request: HttpRequest<any>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<any> {

  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    // Check if we have a refresh token before attempting refresh
    const refreshToken = authService.getRefreshToken();
    if (!refreshToken) {
      isRefreshing = false;
      authService.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return authService.refreshToken().pipe(
      switchMap((response: LoginResponse) => {
        isRefreshing = false;
        // Get the new token from the response based on your LoginResponse structure
        const newToken = response.token;
        refreshTokenSubject.next(newToken);

        // Retry the original request with the new token
        return next(request.clone({
          setHeaders: {
            Authorization: `Bearer ${newToken}`
          }
        }));
      }),
      catchError(err => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.logout();
        return throwError(() => err);
      })
    );
  } else {
    // If refresh is already in progress, wait for it to complete
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(jwt => {
        return next(request.clone({
          setHeaders: {
            Authorization: `Bearer ${jwt}`
          }
        }));
      })
    );
  }
}

// Alternative version with better error handling and logging
export const authInterceptorWithLogging: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  // Skip auth for certain endpoints (like login, register, etc.)
  const skipAuth = req.url.includes('/login') ||
                   req.url.includes('/register') ||
                   req.url.includes('/refresh');

  if (!skipAuth && authService.isAuthenticated) {
    const token = authService.getToken();
    if (token) {
      console.log('Adding auth token to request:', req.url);
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
  }

  return next(req).pipe(
    catchError(error => {
      console.error('HTTP Error:', error);

      if (error instanceof HttpErrorResponse && error.status === 401 && !skipAuth) {
        console.log('401 Unauthorized - attempting token refresh');
        return handle401ErrorWithLogging(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function handle401ErrorWithLogging(
  request: HttpRequest<any>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<any> {

  if (!isRefreshing) {
    console.log('Starting token refresh process');
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const refreshToken = authService.getRefreshToken();
    if (!refreshToken) {
      console.log('No refresh token available - logging out');
      isRefreshing = false;
      authService.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return authService.refreshToken().pipe(
      switchMap((response: LoginResponse) => {
        console.log('Token refresh successful');
        isRefreshing = false;
        const newToken = response.token;
        refreshTokenSubject.next(newToken);

        // Retry the original request with the new token
        return next(request.clone({
          setHeaders: {
            Authorization: `Bearer ${newToken}`
          }
        }));
      }),
      catchError(err => {
        console.error('Token refresh failed:', err);
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.logout();
        return throwError(() => err);
      })
    );
  } else {
    console.log('Token refresh already in progress - waiting');
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(jwt => {
        console.log('Using refreshed token for retry');
        return next(request.clone({
          setHeaders: {
            Authorization: `Bearer ${jwt}`
          }
        }));
      })
    );
  }
}
