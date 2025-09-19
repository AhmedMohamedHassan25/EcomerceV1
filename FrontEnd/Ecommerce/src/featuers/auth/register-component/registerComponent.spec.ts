import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Register } from './registerComponent';
import { AuthService } from '../../../services/auth-service';
import { RegisterRequest } from '../../../models/User';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let mockAuthService: jest.Mocked<AuthService>;
  let mockRouter: jest.Mocked<Router>;

  beforeEach(async () => {
    // Create mock services
    mockAuthService = {
      register: jest.fn(),
      get isAuthenticated() {
        return this._isAuthenticated ?? false;
      },
      _isAuthenticated: false
    } as any;
    mockRouter = {
      navigate: jest.fn()
    } as any;



    await TestBed.configureTestingModule({
      imports: [Register, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
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
      expect(component.registerForm).toBeDefined();
      expect(component.registerForm.get('username')?.value).toBe('');
      expect(component.registerForm.get('email')?.value).toBe('');
      expect(component.registerForm.get('password')?.value).toBe('');
      expect(component.registerForm.get('confirmPassword')?.value).toBe('');
    });

    it('should redirect to products if already authenticated', () => {
        (mockAuthService as any).isAuthenticated = true;
      component.ngOnInit();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
    });

    it('should not redirect if not authenticated', () => {
      (mockAuthService as any).isAuthenticated = true;
      mockRouter.navigate.mockClear();
      component.ngOnInit();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('Form Validation', () => {
    it('should validate required fields', () => {
      const usernameControl = component.registerForm.get('username');
      const emailControl = component.registerForm.get('email');
      const passwordControl = component.registerForm.get('password');
      const confirmPasswordControl = component.registerForm.get('confirmPassword');

      expect(usernameControl?.hasError('required')).toBeTruthy();
      expect(emailControl?.hasError('required')).toBeTruthy();
      expect(passwordControl?.hasError('required')).toBeTruthy();
      expect(confirmPasswordControl?.hasError('required')).toBeTruthy();
    });

    it('should validate username length and pattern', () => {
      const usernameControl = component.registerForm.get('username');

      // Test minLength
      usernameControl?.setValue('ab');
      expect(usernameControl?.hasError('minlength')).toBeTruthy();

      // Test maxLength
      usernameControl?.setValue('a'.repeat(51));
      expect(usernameControl?.hasError('maxlength')).toBeTruthy();

      // Test pattern - invalid characters
      usernameControl?.setValue('user-name!');
      expect(usernameControl?.hasError('pattern')).toBeTruthy();

      // Test valid username
      usernameControl?.setValue('valid_user123');
      expect(usernameControl?.valid).toBeTruthy();
    });

    it('should validate email format', () => {
      const emailControl = component.registerForm.get('email');

      // Invalid email
      emailControl?.setValue('invalid-email');
      expect(emailControl?.hasError('email')).toBeTruthy();

      // Valid email
      emailControl?.setValue('user@example.com');
      expect(emailControl?.valid).toBeTruthy();
    });

    it('should validate password complexity', () => {
      const passwordControl = component.registerForm.get('password');

      // Too short
      passwordControl?.setValue('abc');
      expect(passwordControl?.hasError('minlength')).toBeTruthy();

      // Missing requirements
      passwordControl?.setValue('password');
      expect(passwordControl?.hasError('pattern')).toBeTruthy();

      // Valid password
      passwordControl?.setValue('Password123!');
      expect(passwordControl?.valid).toBeTruthy();
    });

    it('should validate password confirmation match', () => {
      component.registerForm.patchValue({
        password: 'Password123!',
        confirmPassword: 'DifferentPassword123!'
      });

      expect(component.registerForm.hasError('passwordMismatch')).toBeTruthy();

      component.registerForm.patchValue({
        confirmPassword: 'Password123!'
      });

      expect(component.registerForm.hasError('passwordMismatch')).toBeFalsy();
    });
  });

  describe('Form Getters', () => {
    it('should return correct form controls', () => {
      expect(component.username).toBe(component.registerForm.get('username'));
      expect(component.email).toBe(component.registerForm.get('email'));
      expect(component.password).toBe(component.registerForm.get('password'));
      expect(component.confirmPassword).toBe(component.registerForm.get('confirmPassword'));
    });
  });

  describe('Password Visibility Toggle', () => {
    it('should toggle password visibility', () => {
      expect(component.hidePassword).toBeTruthy();
      component.togglePasswordVisibility();
      expect(component.hidePassword).toBeFalsy();
      component.togglePasswordVisibility();
      expect(component.hidePassword).toBeTruthy();
    });

    it('should toggle confirm password visibility', () => {
      expect(component.hideConfirmPassword).toBeTruthy();
      component.toggleConfirmPasswordVisibility();
      expect(component.hideConfirmPassword).toBeFalsy();
      component.toggleConfirmPasswordVisibility();
      expect(component.hideConfirmPassword).toBeTruthy();
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      // Set up valid form data
      component.registerForm.patchValue({
        username: 'testuser',
        email: 'test@example.com',
        password: 'Password123!',
        confirmPassword: 'Password123!'
      });
    });

    it('should submit valid form successfully (with User response)', () => {
      const mockUser = {
        userName: 'testuser',
        email: 'test@example.com'
      }; // شكل الـ User

      mockAuthService.register.mockReturnValue(of(mockUser as any));

      component.onSubmit();

      expect(component.isLoading).toBeFalsy();
      expect(mockAuthService.register).toHaveBeenCalledWith({
        username: 'testuser',
        email: 'test@example.com',
        password: 'Password123!'
      } as RegisterRequest);
      expect(component.successMessage)
        .toBe('Registration successful! Please sign in with your credentials.');
    });

    it('should submit valid form successfully (with message response)', () => {
      const mockResponse = { message: 'Registration successful' };
      mockAuthService.register.mockReturnValue(of(mockResponse) as any);

      component.onSubmit();

      expect(component.isLoading).toBeFalsy();
      expect(component.successMessage)
        .toBe('Registration successful! Please sign in with your credentials.');
    });

    it('should navigate to login after successful registration', (done) => {
      const mockResponse = { message: 'Registration successful' };
      mockAuthService.register.mockReturnValue(of(mockResponse) as any);

      component.onSubmit();

      setTimeout(() => {
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
        done();
      }, 2100);
    });

    it('should handle registration error', () => {
      const errorMessage = 'Registration failed';
      mockAuthService.register.mockReturnValue(throwError(() => new Error(errorMessage)));

      component.onSubmit();

      expect(component.isLoading).toBeFalsy();
      expect(component.errorMessage).toBe(errorMessage);
    });

    it('should not submit invalid form', () => {
      component.registerForm.patchValue({
        username: '',
        email: '',
        password: '',
        confirmPassword: ''
      });

      component.onSubmit();

      expect(mockAuthService.register).not.toHaveBeenCalled();
      expect(component.registerForm.get('username')?.touched).toBeTruthy();
    });

    it('should set loading state during submission', () => {
      mockAuthService.register.mockReturnValue(of({}) as any);

      component.onSubmit();

      // Should be set to true immediately, then false after completion
      expect(component.isLoading).toBeFalsy(); // After completion
    });
  });


  describe('Error Messages', () => {
    it('should return required error message', () => {
      const usernameControl = component.registerForm.get('username');
      usernameControl?.markAsTouched();

      const errorMessage = component.getFieldErrorMessage('username');
      expect(errorMessage).toBe('Username is required');
    });

    it('should return minlength error message', () => {
      const usernameControl = component.registerForm.get('username');
      usernameControl?.setValue('ab');
      usernameControl?.markAsTouched();

      const errorMessage = component.getFieldErrorMessage('username');
      expect(errorMessage).toBe('Username must be at least 3 characters long');
    });

    it('should return maxlength error message', () => {
      const emailControl = component.registerForm.get('email');
      emailControl?.setValue('a'.repeat(101) + '@example.com');
      emailControl?.markAsTouched();

      const errorMessage = component.getFieldErrorMessage('email');
      expect(errorMessage).toBe('Email cannot exceed 100 characters');
    });

    it('should return email error message', () => {
      const emailControl = component.registerForm.get('email');
      emailControl?.setValue('invalid-email');
      emailControl?.markAsTouched();

      const errorMessage = component.getFieldErrorMessage('email');
      expect(errorMessage).toBe('Please enter a valid email address');
    });

    it('should return username pattern error message', () => {
      const usernameControl = component.registerForm.get('username');
      usernameControl?.setValue('user-name!');
      usernameControl?.markAsTouched();

      const errorMessage = component.getFieldErrorMessage('username');
      expect(errorMessage).toBe('Username can only contain letters, numbers, and underscores');
    });

    it('should return password pattern error message', () => {
      const passwordControl = component.registerForm.get('password');
      passwordControl?.setValue('weakpassword');
      passwordControl?.markAsTouched();

      const errorMessage = component.getFieldErrorMessage('password');
      expect(errorMessage).toBe('Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character');
    });

    it('should return password mismatch error message', () => {
      component.registerForm.patchValue({
        password: 'Password123!',
        confirmPassword: 'DifferentPassword123!'
      });

      const errorMessage = component.getFieldErrorMessage('confirmPassword');
      expect(errorMessage).toBe('Passwords do not match');
    });

    it('should return empty string for valid field', () => {
      const usernameControl = component.registerForm.get('username');
      usernameControl?.setValue('validuser');

      const errorMessage = component.getFieldErrorMessage('username');
      expect(errorMessage).toBe('');
    });
  });

  describe('Navigation', () => {
    it('should navigate to login page', () => {
      component.navigateToLogin();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('Component Cleanup', () => {
    it('should complete destroy subject on ngOnDestroy', () => {
      const destroySpy = jest.spyOn(component['destroy$'], 'next');
      const completeSpy = jest.spyOn(component['destroy$'], 'complete');

      component.ngOnDestroy();

      expect(destroySpy).toHaveBeenCalled();
      expect(completeSpy).toHaveBeenCalled();
    });
  });

  describe('Private Methods', () => {
    it('should mark all form controls as touched', () => {
      const usernameControl = component.registerForm.get('username');
      const emailControl = component.registerForm.get('email');
      const passwordControl = component.registerForm.get('password');
      const confirmPasswordControl = component.registerForm.get('confirmPassword');

      expect(usernameControl?.touched).toBeFalsy();
      expect(emailControl?.touched).toBeFalsy();
      expect(passwordControl?.touched).toBeFalsy();
      expect(confirmPasswordControl?.touched).toBeFalsy();

      component['markFormGroupTouched']();

      expect(usernameControl?.touched).toBeTruthy();
      expect(emailControl?.touched).toBeTruthy();
      expect(passwordControl?.touched).toBeTruthy();
      expect(confirmPasswordControl?.touched).toBeTruthy();
    });
  });
});
