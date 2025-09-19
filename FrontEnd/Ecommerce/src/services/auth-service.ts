import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../enviroments/environment.development';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { LoginRequest, LoginResponse, RefreshTokenRequest, RegisterRequest, User } from '../models/User';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.BaseUrl}/Auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenRefreshTimer: any;
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, @Inject(PLATFORM_ID) private platformId: Object) {
    if (isPlatformBrowser(this.platformId)) {
      this.loadUserFromStorage();
      this.scheduleTokenRefresh();
    }
  }

  // ===== Public Methods =====
  public login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        console.log('Login response:', response);

        const loginResponse: LoginResponse = {
          token: response.value.accessToken,
          refreshToken: response.value.refreshToken,
          expiresAt: response.value.expires,

          user: response.value.user
        };
                console.log('expiresAt:', response.value.refreshToken);


        this.setAuthData(loginResponse);
        this.scheduleTokenRefresh();
      }),
      catchError(this.handleError)
    );
  }

  public register(userData: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/register`, userData).pipe(
      catchError(this.handleError)
    );
  }

  public refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }
    const request: RefreshTokenRequest = { refreshToken };

    return this.http.post<any>(`${this.apiUrl}/refresh`, request).pipe(
      tap(response => {
        // تحويل الاستجابة للتنسيق المطلوب
        const loginResponse: LoginResponse = {
          token: response.value.accessToken,
          refreshToken: response.value.refreshToken,
          expiresAt: response.value.expires,
          user: response.value.user
        };

        this.setAuthData(loginResponse);
        this.scheduleTokenRefresh();
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  public logout(): void {
    this.clearAuthData();
    this.clearTokenRefreshTimer();
    this.currentUserSubject.next(null);
  }

  public clearTokenRefreshTimer(): void {
    if (this.tokenRefreshTimer) {
      clearTimeout(this.tokenRefreshTimer);
      this.tokenRefreshTimer = null;
    }
  }

  public getRefreshToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    return localStorage.getItem('refresh_token');
  }

  private setAuthData(response: LoginResponse): void {
    if (!isPlatformBrowser(this.platformId)) return;

    let expiresAt: Date;
    if (typeof response.expiresAt === 'string') {
      expiresAt = new Date(response.expiresAt);
    } else {
      expiresAt = response.expiresAt;
    }

    localStorage.setItem('access_token', response.token);
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('token_expires_at', expiresAt.toISOString());
    localStorage.setItem('current_user', JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);

    console.log('Token expires at:', expiresAt);
    console.log('Current time:', new Date());
  }

  private clearAuthData(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expires_at');
    localStorage.removeItem('current_user');
  }

  private loadUserFromStorage(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const userJson = localStorage.getItem('current_user');
    const token = this.getToken();
    const expiresAt = localStorage.getItem('token_expires_at');

    if (userJson && token && expiresAt) {
      const expireDate = new Date(expiresAt);
      const now = new Date();

      if (expireDate > now) {
        const user = JSON.parse(userJson);
        this.currentUserSubject.next(user);
      } else {
        this.clearAuthData();
      }
    }
  }

  private scheduleTokenRefresh(): void {
    if (!isPlatformBrowser(this.platformId)) {
      console.log("Not in browser platform");
      return;
    }

    this.clearTokenRefreshTimer();

    const expiresAtString = localStorage.getItem('token_expires_at');
    if (!expiresAtString) {
      console.log("No expiration time found");
      return;
    }

    const expiresAt = new Date(expiresAtString);
    const now = new Date();

    const timeUntilExpiry = expiresAt.getTime() - now.getTime();

    const refreshTime = timeUntilExpiry - (5 * 60 * 1000);

    console.log(`Token expires at: ${expiresAt}`);
    console.log(`Current time: ${now}`);
    console.log(`Time until expiry: ${timeUntilExpiry}ms`);
    console.log(`Will refresh in: ${refreshTime}ms`);

    if (refreshTime > 0 && this.getRefreshToken()) {
      this.tokenRefreshTimer = setTimeout(() => {
        console.log('Attempting to refresh token...');
        this.refreshToken().subscribe({
          next: () => console.log('Token refreshed successfully'),
          error: (error) => {
            console.error('Token refresh failed:', error);
            this.logout();
          }
        });
      }, refreshTime);
    } else if (timeUntilExpiry <= 0) {
      console.log('Token already expired, logging out');
      this.logout();
    }
  }

  public getToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    return localStorage.getItem('access_token');
  }

  public get isAuthenticated(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }

    const token = this.getToken();
    const user = this.currentUserValue;
    const expiresAt = localStorage.getItem('token_expires_at');

    if (!token || !user || !expiresAt) {
      return false;
    }

    const expireDate = new Date(expiresAt);
    const now = new Date();

    return expireDate > now;
  }

 public get currentUserValue(): User | null {
  const value = this.currentUserSubject.value;
  if (value) {
    console.log("username:", value.userName);
  } else {
    console.log("No current user");
  }
  return value;
}


  private handleError(error: any): Observable<never> {
    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else if (error.status) {
      switch (error.status) {
        case 401:
          errorMessage = 'Invalid credentials';
          break;
        case 403:
          errorMessage = 'Access forbidden';
          break;
        case 404:
          errorMessage = 'Service not found';
          break;
        case 500:
          errorMessage = 'Server error';
          break;
        default:
          errorMessage = error.error?.message || `Error Code: ${error.status}`;
      }
    }
    return throwError(() => new Error(errorMessage));
  }
}
