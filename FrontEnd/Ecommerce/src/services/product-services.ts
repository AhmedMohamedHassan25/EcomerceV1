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
export class ProductService {

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService
  ) {}

  getAllProducts(): Observable<ProductResponse> {


    return this.httpClient.get<ProductResponse>(`${environment.BaseUrl}/Products`).pipe(
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

  return this.httpClient.get<Product>(`$${environment.BaseUrl}/Products/${id}`).pipe(
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

  return this.httpClient.post<Product>(`${environment.BaseUrl}/Products/`, item).pipe(
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

          console.log(' Product updated :', item);

  return this.httpClient.put<Product>(`${environment.BaseUrl}/Products/${id}`,item).pipe(
    tap(response => {
      console.log(' Product updated successfully:', response);
      alert("Product updated successfully");
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
    // console.log(' Attempting to delete product:', id, 'at:', `${this.apiUrl}/${id}`);

    return this.httpClient.delete<void>(`${environment.BaseUrl}/Products/${id}`).pipe(
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


  updateProductImage(id: number, imageFile: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', imageFile);

  console.log('Updating product image for ID:', id);
  console.log('Image file:', imageFile.name);

  return this.httpClient.post(`${environment.BaseUrl}/Products/${id}/image`, formData).pipe(
    tap(response => {
      console.log('Product image updated successfully:', response);
      alert("Product image updated successfully");
    }),
    catchError(error => {
      console.error('Error updating product image:', error);
      console.error('Error details:', {
        status: error.status,
        message: error.message,
        url: error.url,
        productId: id,
        fileName: imageFile?.name
      });
      throw error;
    })
  );
}


}
