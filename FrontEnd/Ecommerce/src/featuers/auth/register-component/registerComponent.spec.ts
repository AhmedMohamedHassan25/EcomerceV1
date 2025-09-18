// register.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Register } from './registerComponent';
import { AuthService } from '../../../services/auth-service';
import { RegisterRequest } from '../../../models/User';


describe('RegisterComponent', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let authService: jest.Mocked<AuthService>;
  let router: jest.Mocked<Router>;

  const mockUser = {
    id: 1,
    username: 'testuser',
    email: 'test@example.com'
  };

  beforeEach(async () => {
    const authServiceMock: Partial<jest.Mocked<AuthService>> = {
      register: jest.fn(),
      isAuthenticated: false
    };

    const routerMock: Partial<jest.Mocked<Router>> = {
      navigate: jest.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [Register],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jest.Mocked<AuthService>;
    router = TestBed.inject(Router) as jest.Mocked<Router>;
  });

  beforeEach(() => {
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty values', () => {
    expect(component.registerForm.get('username')?.value).toBe('');
    expect(component.registerForm.get('email')?.value).toBe('');
    expect(component.registerForm.get('password')?.value).toBe('');
    expect(component.registerForm.get('confirmPassword')?.value).toBe('');
  });

  it('should require all fields', () => {
    const controls = ['username', 'email', 'password', 'confirmPassword'];
    controls.forEach(controlName => {
      const control = component.registerForm.get(controlName);
      expect(control?.hasError('required')).toBeTruthy();
    });
  });

  it('should validate email format', () => {
    const emailControl = component.registerForm.get('email');
    emailControl?.setValue('invalid-email');
    expect(emailControl?.hasError('email')).toBeTruthy();

    emailControl?.setValue('valid@example.com');
    expect(emailControl?.hasError('email')).toBeFalsy();
  });

  it('should validate username pattern', () => {
    const usernameControl = component.registerForm.get('username');
    usernameControl?.setValue('invalid-username!');
    expect(usernameControl?.hasError('pattern')).toBeTruthy();

    usernameControl?.setValue('valid_username123');
    expect(usernameControl?.hasError('pattern')).toBeFalsy();
  });

  it('should validate password pattern', () => {
    const passwordControl = component.registerForm.get('password');
    passwordControl?.setValue('weak');
    expect(passwordControl?.hasError('pattern')).toBeTruthy();

    passwordControl?.setValue('StrongPassword123!');
    expect(passwordControl?.hasError('pattern')).toBeFalsy();
  });

  it('should validate password confirmation', () => {
    component.registerForm.patchValue({
      password: 'password123',
      confirmPassword: 'different'
    });

    expect(component.registerForm.hasError('passwordMismatch')).toBeTruthy();

    component.registerForm.patchValue({
      confirmPassword: 'password123'
    });

    expect(component.registerForm.hasError('passwordMismatch')).toBeFalsy();
  });

  it('should register user with valid form', () => {
    authService.register.mockReturnValue(of(mockUser));

    component.registerForm.patchValue({
      username: 'testuser',
      email: 'test@example.com',
      password: 'Password123!',
      confirmPassword: 'Password123!'
    });

    component.onSubmit();

    const expectedUserData: RegisterRequest = {
      username: 'testuser',
      email: 'test@example.com',
      password: 'Password123!'
    };

    expect(authService.register).toHaveBeenCalledWith(expectedUserData);
  });

  it('should show success message and navigate on successful registration', (done) => {
    authService.register.mockReturnValue(of(mockUser));

    component.registerForm.patchValue({
      username: 'testuser',
      email: 'test@example.com',
      password: 'Password123!',
      confirmPassword: 'Password123!'
    });

    component.onSubmit();

    expect(component.successMessage).toBeTruthy();
    expect(component.isLoading).toBe(false);

    setTimeout(() => {
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
      done();
    }, 2100);
  });

  it('should show error message on failed registration', () => {
    authService.register.mockReturnValue(throwError(() => new Error('Registration failed')));

    component.registerForm.patchValue({
      username: 'testuser',
      email: 'test@example.com',
      password: 'Password123!',
      confirmPassword: 'Password123!'
    });

    component.onSubmit();

    expect(component.errorMessage).toBe('Registration failed');
    expect(component.isLoading).toBe(false);
  });

  it('should toggle password visibility', () => {
    expect(component.hidePassword).toBeTruthy();
    component.togglePasswordVisibility();
    expect(component.hidePassword).toBeFalsy();
  });

  it('should toggle confirm password visibility', () => {
    expect(component.hideConfirmPassword).toBeTruthy();
    component.toggleConfirmPasswordVisibility();
    expect(component.hideConfirmPassword).toBeFalsy();
  });

  it('should navigate to login', () => {
    component.navigateToLogin();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should return correct error messages', () => {
    const usernameControl = component.registerForm.get('username');
    usernameControl?.setValue('');
    usernameControl?.markAsTouched();

    expect(component.getFieldErrorMessage('username')).toBe('Username is required');

    usernameControl?.setValue('ab');
    expect(component.getFieldErrorMessage('username')).toBe('Username must be at least 3 characters long');

    usernameControl?.setValue('invalid!');
    expect(component.getFieldErrorMessage('username')).toBe('Username can only contain letters, numbers, and underscores');
  });

  it('should redirect if already authenticated', () => {
    jest.spyOn(authService, 'isAuthenticated', 'get').mockReturnValue(true);
    component.ngOnInit();
    expect(router.navigate).toHaveBeenCalledWith(['/products']);
  });
});
