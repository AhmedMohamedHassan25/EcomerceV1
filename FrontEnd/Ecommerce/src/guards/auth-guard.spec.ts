import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthGuard } from './auth-guard';
import { AuthService } from '../services/auth-service';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authServiceMock: Partial<AuthService>;
  let routerMock: Partial<Router>;

  beforeEach(() => {
    authServiceMock = { isAuthenticated:  true };
    routerMock = { navigate: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    });

    guard = TestBed.inject(AuthGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  it('should allow activation when authenticated', () => {
    // Pass empty mocks for route and state if not used
    expect(guard.canActivate({} as any, {} as any)).toBe(true);
  });
});
