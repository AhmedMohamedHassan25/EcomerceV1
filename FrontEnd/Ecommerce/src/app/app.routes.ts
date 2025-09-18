import { Routes } from '@angular/router';
import { AuthGuard } from '../guards/auth-guard';
import { LoginComponent } from '../featuers/auth/login-component/LoginComponent';
import { ProductListComponent } from '../featuers/products/product-list/product-list';
import { Register } from '../featuers/auth/register-component/registerComponent';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'register', component: Register, title: 'register' },
  { path: 'products', component: ProductListComponent, canActivate: [AuthGuard], title: 'Products' },
  { path: '**', redirectTo: 'products' }
];
