import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../services/auth-service';
import { LoginComponent } from './LoginComponent';
import { LoginRequest, LoginResponse } from '../../../models/User';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jest.Mocked<AuthService>;
  let router: jest.Mocked<Router>;
  let activatedRoute: ActivatedRoute;

  const mockLoginResponse: LoginResponse = {
    token: 'mock-token',
    refreshToken: 'mock-refresh-token',
    user: { id: 1, username: 'testuser', email: 'test@example.com' },
    expiresAt: new Date()
  };

  beforeEach(async () => {
    const authServiceMock: Partial<jest.Mocked<AuthService>> = {
      login: jest.fn(),
      isAuthenticated: false
    };
    const routerMock: Partial<jest.Mocked<Router>> = {
      navigate: jest.fn()
    };
    const activatedRouteMock = {
      snapshot: { queryParams: { returnUrl: '/products' } }
    };

    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jest.Mocked<AuthService>;
    router = TestBed.inject(Router) as jest.Mocked<Router>;
    activatedRoute = TestBed.inject(ActivatedRoute);
  });

  beforeEach(() => {
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty values', () => {
    expect(component.loginForm.get('username')?.value).toBe('');
    expect(component.loginForm.get('password')?.value).toBe('');
  });

  it('should require username and password', () => {
    expect(component.loginForm.get('username')?.hasError('required')).toBeTruthy();
    expect(component.loginForm.get('password')?.hasError('required')).toBeTruthy();
  });

  it('should validate minimum length for username and password', () => {
    component.loginForm.get('username')?.setValue('ab');
    component.loginForm.get('password')?.setValue('12345');

    expect(component.loginForm.get('username')?.hasError('minlength')).toBeTruthy();
    expect(component.loginForm.get('password')?.hasError('minlength')).toBeTruthy();
  });

  it('should call authService.login on valid form submission', () => {
    authService.login.mockReturnValue(of(mockLoginResponse));

    component.loginForm.patchValue({ username: 'testuser', password: 'password123' });
    component.onSubmit();

    const expectedCredentials: LoginRequest = { username: 'testuser', password: 'password123' };
    expect(authService.login).toHaveBeenCalledWith(expectedCredentials);
  });

  it('should navigate to returnUrl on successful login', () => {
    authService.login.mockReturnValue(of(mockLoginResponse));
    component.loginForm.patchValue({ username: 'testuser', password: 'password123' });
    component.onSubmit();

    expect(router.navigate).toHaveBeenCalledWith(['/products']);
  });

  it('should show error message on failed login', () => {
    authService.login.mockReturnValue(throwError(() => new Error('Invalid credentials')));
    component.loginForm.patchValue({ username: 'testuser', password: 'wrongpassword' });

    component.onSubmit();

    expect(component.errorMessage).toBe('Invalid credentials');
    expect(component.isLoading).toBe(false);
  });

  it('should toggle password visibility', () => {
    expect(component.hidePassword).toBe(true);
    component.togglePasswordVisibility();
    expect(component.hidePassword).toBe(false);
  });

  it('should mark form as touched on invalid submission', () => {
    const markFormSpy = jest.spyOn(component as any, 'markFormGroupTouched');
    component.onSubmit();
    expect(markFormSpy).toHaveBeenCalled();
  });

  it('should return correct error messages', () => {
    const usernameControl = component.loginForm.get('username');
    usernameControl?.setValue('');
    usernameControl?.markAsTouched();
    expect(component.getFieldErrorMessage('username')).toBe('Username is required');

    usernameControl?.setValue('ab');
    expect(component.getFieldErrorMessage('username')).toBe('Username must be at least 3 characters long');
  });

  it('should navigate to register page', () => {
    component.navigateToRegister();
    expect(router.navigate).toHaveBeenCalledWith(['/register']);
  });

  it('should redirect if already authenticated', () => {
    // Jest way to spy on getter
    jest.spyOn(authService, 'isAuthenticated', 'get').mockReturnValue(true);

    component.ngOnInit();
    expect(router.navigate).toHaveBeenCalledWith(['/products']);
  });
});
