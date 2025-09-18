import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../services/auth-service';
import { LoginComponent } from '../../../featuers/auth/login-component/LoginComponent';
import { LoginRequest, LoginResponse } from '../../../models/User';


describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: any;

  const mockLoginResponse: LoginResponse
   = {
    token: 'mock-token',
    refreshToken: 'mock-refresh-token',
    user: {
      id: 1,
      username: 'testuser',
      email: 'test@example.com'
    },
    expiresAt: new Date()
  };

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['login'], {
      isAuthenticated: false
    });
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const activatedRouteSpy = {
      snapshot: {
        queryParams: { returnUrl: '/products' }
      }
    };

    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRouteSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
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
    const usernameControl = component.loginForm.get('username');
    const passwordControl = component.loginForm.get('password');

    expect(usernameControl?.hasError('required')).toBeTruthy();
    expect(passwordControl?.hasError('required')).toBeTruthy();
  });

  it('should validate minimum length for username and password', () => {
    const usernameControl = component.loginForm.get('username');
    const passwordControl = component.loginForm.get('password');

    usernameControl?.setValue('ab');
    passwordControl?.setValue('12345');

    expect(usernameControl?.hasError('minlength')).toBeTruthy();
    expect(passwordControl?.hasError('minlength')).toBeTruthy();
  });

  it('should call authService.login on valid form submission', () => {
    authService.login.and.returnValue(of(mockLoginResponse));

    component.loginForm.patchValue({
      username: 'testuser',
      password: 'password123'
    });

    component.onSubmit();

    const expectedCredentials: LoginRequest = {
      username: 'testuser',
      password: 'password123'
    };

    expect(authService.login).toHaveBeenCalledWith(expectedCredentials);
  });

  it('should navigate to returnUrl on successful login', () => {
    authService.login.and.returnValue(of(mockLoginResponse));

    component.loginForm.patchValue({
      username: 'testuser',
      password: 'password123'
    });

    component.onSubmit();

    expect(router.navigate).toHaveBeenCalledWith(['/products']);
  });

  it('should show error message on failed login', () => {
    authService.login.and.returnValue(throwError(() => new Error('Invalid credentials')));

    component.loginForm.patchValue({
      username: 'testuser',
      password: 'wrongpassword'
    });

    component.onSubmit();

    expect(component.errorMessage).toBe('Invalid credentials');
    expect(component.isLoading).toBeFalse();
  });

  it('should toggle password visibility', () => {
    expect(component.hidePassword).toBeTruthy();

    component.togglePasswordVisibility();

    expect(component.hidePassword).toBeFalsy();
  });

  it('should mark form as touched on invalid submission', () => {
    spyOn(component as any, 'markFormGroupTouched');

    component.onSubmit();

    expect((component as any).markFormGroupTouched).toHaveBeenCalled();
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
  spyOnProperty(authService, 'isAuthenticated').and.returnValue(true);

    component.ngOnInit();

    expect(router.navigate).toHaveBeenCalledWith(['/products']);
  });
  
});
