import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './LoginComponent';
import { AuthService } from '../../../services/auth-service';
import { LoginRequest, LoginResponse, User } from '../../../models/User';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: jest.Mocked<AuthService>;
  let mockRouter: jest.Mocked<Router>;
  let mockActivatedRoute: any;

  // 🔹 helper function to create mock LoginResponse
  function createMockLoginResponse(userName = 'testuser'): LoginResponse {
    return {
      token: 'test-token',
      refreshToken: 'test-refresh-token',
      user: {
        userId: 1,
        userName,
        email: `${userName}@example.com`,
        createdAt: new Date()
      },
      expiresAt: new Date('2024-12-31T23:59:59Z')
    };
  }

  beforeEach(async () => {
    mockAuthService = {
      login: jest.fn()
    } as any;

    Object.defineProperty(mockAuthService, 'isAuthenticated', {
      get: jest.fn(() => false),
      configurable: true
    });

    mockRouter = {
      navigate: jest.fn()
    } as any;

    mockActivatedRoute = {
      snapshot: {
        queryParams: {}
      }
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with empty values', () => {
      expect(component.loginForm.get('username')?.value).toBe('');
      expect(component.loginForm.get('password')?.value).toBe('');
    });

    it('should set default return URL to /products', () => {
      component.ngOnInit();
      expect(component.returnUrl).toBe('/products');
    });

    it('should use query param returnUrl if provided', () => {
      mockActivatedRoute.snapshot.queryParams['returnUrl'] = '/dashboard';
      component.ngOnInit();
      expect(component.returnUrl).toBe('/dashboard');
    });

    it('should redirect if already authenticated', () => {
      (Object.getOwnPropertyDescriptor(mockAuthService, 'isAuthenticated')?.get as jest.Mock)
        .mockReturnValue(true);

      component.returnUrl = '/dashboard';
      component.ngOnInit();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });

  describe('Form Validation', () => {
    it('should validate required fields', () => {
      const usernameControl = component.loginForm.get('username');
      const passwordControl = component.loginForm.get('password');

      expect(usernameControl?.hasError('required')).toBeTruthy();
      expect(passwordControl?.hasError('required')).toBeTruthy();
    });

    it('should validate username minlength', () => {
      const usernameControl = component.loginForm.get('username');
      usernameControl?.setValue('ab');
      expect(usernameControl?.hasError('minlength')).toBeTruthy();
      usernameControl?.setValue('abc');
      expect(usernameControl?.hasError('minlength')).toBeFalsy();
    });

    it('should validate password minlength', () => {
      const passwordControl = component.loginForm.get('password');
      passwordControl?.setValue('12345');
      expect(passwordControl?.hasError('minlength')).toBeTruthy();
      passwordControl?.setValue('123456');
      expect(passwordControl?.hasError('minlength')).toBeFalsy();
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      component.loginForm.patchValue({
        username: 'testuser',
        password: 'password123'
      });
      component.returnUrl = '/dashboard';
    });

    it('should submit valid form successfully', () => {
      const mockResponse = createMockLoginResponse();
      mockAuthService.login.mockReturnValue(of(mockResponse));

      component.onSubmit();

      expect(mockAuthService.login).toHaveBeenCalledWith({
        username: 'testuser',
        password: 'password123'
      } as LoginRequest);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should handle login error', () => {
      mockAuthService.login.mockReturnValue(throwError(() => new Error('Invalid credentials')));

      component.onSubmit();

      expect(component.errorMessage).toBe('Invalid credentials');
      expect(component.loginForm.get('password')?.value).toBe('');
    });

    it('should not submit invalid form', () => {
      component.loginForm.patchValue({ username: '', password: '' });
      component.onSubmit();
      expect(mockAuthService.login).not.toHaveBeenCalled();
    });
  });

  describe('Error Messages', () => {
    it('should return required message for username', () => {
      const control = component.loginForm.get('username');
      control?.markAsTouched();
      expect(component.getFieldErrorMessage('username')).toBe('Username is required');
    });

    it('should return minlength message for password', () => {
      const control = component.loginForm.get('password');
      control?.setValue('123');
      control?.markAsTouched();
      expect(component.getFieldErrorMessage('password')).toBe('Password must be at least 6 characters long');
    });

    it('should return empty string if valid', () => {
      const control = component.loginForm.get('username');
      control?.setValue('validuser');
      expect(component.getFieldErrorMessage('username')).toBe('');
    });
  });

  describe('Integration Tests', () => {
    it('should handle complete login flow with return URL', () => {
      mockActivatedRoute.snapshot.queryParams['returnUrl'] = '/profile';
      const mockResponse = createMockLoginResponse('testuser');

      mockAuthService.login.mockReturnValue(of(mockResponse));

      component.ngOnInit();
      component.loginForm.patchValue({
        username: 'testuser',
        password: 'password123'
      });

      component.onSubmit();

      expect(component.returnUrl).toBe('/profile');
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/profile']);
    });
  });
});
