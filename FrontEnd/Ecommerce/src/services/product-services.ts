import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Product, ProductResponse } from '../models/Products';
import { environment } from '../enviroments/environment.development';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root'
})
export class ProductService { // غيرت الاسم ليكون موحد
  private readonly apiUrl = `${environment.BaseUrl}/Products`;

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService
  ) {}

  getAllProducts(): Observable<ProductResponse> {
    console.log('Attempting to fetch products from:', this.apiUrl);
    console.log('Is user authenticated?', this.authService.isAuthenticated);
    console.log('Current token:', this.authService.getToken()?.substring(0, 20) + '...');
    console.log('👤 Current user:', this.authService.currentUserValue);

    return this.httpClient.get<ProductResponse>(this.apiUrl).pipe(
      tap(response => {
        console.log(' Products fetched successfully:', response);
      }),
      catchError(error => {
        console.error('Error fetching products:', error);
        console.error('Error details:', {
          status: error.status,
          message: error.message,
          url: error.url
        });
        throw error;
      })
    );
  }

  getProductById(id: number): Observable<Product> {
  console.log('🔍 Attempting to fetch product by ID:', id, 'from:', `${this.apiUrl}/${id}`);
  console.log('🔑 Is user authenticated?', this.authService.isAuthenticated);
  console.log('🎫 Current token:', this.authService.getToken()?.substring(0, 20) + '...');
  console.log('👤 Current user:', this.authService.currentUserValue);
  return this.httpClient.get<Product>(`${this.apiUrl}/Products/${id}`).pipe(
    tap(response => {
      console.log('✅ Product fetched successfully:', response);
    }),
    catchError(error => {
      console.error(' Error fetching product:', error);
      console.error(' Error details:', {
        status: error.status,
        message: error.message,
        url: error.url,
        productId: id
      });
      throw error;
    })
  );
}
  createProduct(product: Product): Observable<Product> {
     let  item={
        category: product.category,
        name: product.name,
        price:product.price,
        minimumQuantity: product.minimumQuantity,
        discountRate: product.discountRate
    }
  console.log('🆕 Attempting to create product:', product, 'at:', this.apiUrl);
  console.log('🔑 Is user authenticated?', this.authService.isAuthenticated);
  console.log('🎫 Current token:', this.authService.getToken()?.substring(0, 20) + '...');
  console.log('👤 Current user:', this.authService.currentUserValue);
  return this.httpClient.post<Product>(`${this.apiUrl}/Products/`, {item}).pipe(
    tap(response => {
      console.log(' Product created successfully:', response);
    }),
    catchError(error => {
      console.error('Error creating product:', error);
      console.error('Error details:', {
        status: error.status,
        message: error.message,
        url: error.url,
        productData: product
      });
      throw error;
    })
  );
}

  updateProduct(id: number, product: Product): Observable<Product> {
      let  item={
        category: product.category,
        name: product.name,
        price:product.price,
        minimumQuantity: product.minimumQuantity,
        discountRate: product.discountRate
    }

  console.log(' Attempting to update product:', id, 'with data:', product, 'at:', `${this.apiUrl}/${id}`);
  console.log(' Is user authenticated?', this.authService.isAuthenticated);
  console.log('Current token:', this.authService.getToken()?.substring(0, 20) + '...');
  console.log('Current user:', this.authService.currentUserValue);
  return this.httpClient.put<Product>(`${this.apiUrl}/Products/${id}`,{item}).pipe(
    tap(response => {
      console.log(' Product updated successfully:', response);
      prompt("Product updated successfully");
    }),
    catchError(error => {
      console.error(' Error updating product:', error);
      console.error('Error details:', {
        status: error.status,
        message: error.message,
        url: error.url,
        productId: id,
        updateData: product
      });
      throw error;
    })
  );
  }


  deleteProduct(id: number): Observable<void> {
    console.log(' Attempting to delete product:', id, 'at:', `${this.apiUrl}/${id}`);
    console.log(' Is user authenticated?', this.authService.isAuthenticated);
    console.log(' Current token:', this.authService.getToken()?.substring(0, 20) + '...');
    console.log('Current user:', this.authService.currentUserValue);
    return this.httpClient.delete<void>(`${this.apiUrl}/Products/${id}`).pipe(
      tap(() => {
        console.log(' Product deleted successfully, ID:', id);
      }),
      catchError(error => {
        console.error('Error deleting product:', error);
        console.error(' Error details:', {
          status: error.status,
          message: error.message,
          url: error.url,
          productId: id
        });
        throw error;
      })
    );
  }



}
