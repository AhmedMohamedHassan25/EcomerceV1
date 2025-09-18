// import { Injectable } from '@angular/core';
// import { environment } from '../enviroments/environment.development';
// import { BehaviorSubject, catchError, Observable, tap, throwError, map, Subscription } from 'rxjs';
// import { Product, ProductFilter, ProductResponse } from '../models/Products';
// import { HttpClient, HttpParams } from '@angular/common/http';


// @Injectable({
//   providedIn: 'root'
// })
// export class ProductService  {
//   private readonly apiUrl = `${environment.BaseUrl}/Products`;

//   // // State management with BehaviorSubjects
//   // private productsSubject = new BehaviorSubject<Product[]>([]);
//   // private loadingSubject = new BehaviorSubject<boolean>(false);
//   // private categoriesSubject = new BehaviorSubject<string[]>([]);
//   // private currentPageSubject = new BehaviorSubject<number>(1);
//   // private totalPagesSubject = new BehaviorSubject<number>(0);
//   // private totalCountSubject = new BehaviorSubject<number>(0);

//   // // Public observables
//   // public products$ = this.productsSubject.asObservable();
//   // public loading$ = this.loadingSubject.asObservable();
//   // public categories$ = this.categoriesSubject.asObservable();
//   // public currentPage$ = this.currentPageSubject.asObservable();
//   // public totalPages$ = this.totalPagesSubject.asObservable();
//   // public totalCount$ = this.totalCountSubject.asObservable();

//   constructor(private http: HttpClient) {
//     // this.loadCategories();
//   }

//   // GET /api/Products - Get all products with pagination
//   getAllProducts( ):Observable<ProductResponse>
//   {
//     var  content =this.http.get<ProductResponse>(`${environment.BaseUrl}/Products`);

//     console.log(content);
//     return this.http.get<ProductResponse>(`${environment.BaseUrl}/Products`);
//   }


// //   // GET /api/Products/{id} - Get product by ID
// //   getProductById(id: number): Observable<Product> {
// //     return this.http.get<ApiResult<ProductDTO>>(`${this.apiUrl}/${id}`)
// //       .pipe(
// //         catchError(this.handleError),
// //         map(response => {
// //           if (response.isSuccess && response.data) {
// //             return this.mapProductDTOToProduct(response.data);
// //           }
// //           throw new Error(response.message || 'Product not found');
// //         })
// //       );
// //   }

// //   // GET /api/Products/category/{category} - Get products by category
// //   getProductsByCategory(category: string): Observable<Product[]> {
// //     return this.http.get<ApiResult<EntityPaginated<ProductDTO>>>(`${this.apiUrl}/category/${encodeURIComponent(category)}`)
// //       .pipe(
// //         catchError(this.handleError),
// //         map(response => {
// //           if (response.isSuccess && response.data) {
// //             return this.mapProductDTOsToProducts(response.data.items);
// //           }
// //           throw new Error(response.message || 'No products found in this category');
// //         })
// //       );
// //   }

// //   // POST /api/Products - Create new product
// //   createProduct(productData: Omit<Product, 'id' | 'createdAt' | 'updatedAt'>): Observable<Product> {
// //     const createDTO: CreateProductDTO = {
// //       productCode: productData.productCode,
// //       name: productData.name,
// //       category: productData.category,
// //       price: productData.price,
// //       minimumQuantity: productData.minimumQuantity,
// //       discountRate: productData.discountRate
// //     };

// //     return this.http.post<ApiResult<ProductDTO>>(this.apiUrl, createDTO)
// //       .pipe(
// //         tap(response => {
// //           if (response.isSuccess && response.data) {
// //             const newProduct = this.mapProductDTOToProduct(response.data);
// //             const currentProducts = this.productsSubject.value;
// //             this.productsSubject.next([newProduct, ...currentProducts]);
// //           }
// //         }),
// //         catchError(this.handleError),
// //         map(response => {
// //           if (response.isSuccess && response.data) {
// //             return this.mapProductDTOToProduct(response.data);
// //           }
// //           throw new Error(response.message || 'Failed to create product');
// //         })
// //       );
// //   }

// //   // PUT /api/Products/{id} - Update product
// //   updateProduct(id: number, productData: Partial<Product>): Observable<Product> {
// //     const updateDTO: UpdateProductDTO = {
// //       ...(productData.productCode && { productCode: productData.productCode }),
// //       ...(productData.name && { name: productData.name }),
// //       ...(productData.category && { category: productData.category }),
// //       ...(productData.price !== undefined && { price: productData.price }),
// //       ...(productData.minimumQuantity !== undefined && { minimumQuantity: productData.minimumQuantity }),
// //       ...(productData.discountRate !== undefined && { discountRate: productData.discountRate })
// //     };

// //     return this.http.put<ApiResult<ProductDTO>>(`${this.apiUrl}/${id}`, updateDTO)
// //       .pipe(
// //         tap(response => {
// //           if (response.isSuccess && response.data) {
// //             const updatedProduct = this.mapProductDTOToProduct(response.data);
// //             const currentProducts = this.productsSubject.value;
// //             const index = currentProducts.findIndex(p => p.id === id);
// //             if (index !== -1) {
// //               currentProducts[index] = updatedProduct;
// //               this.productsSubject.next([...currentProducts]);
// //             }
// //           }
// //         }),
// //         catchError(this.handleError),
// //         map(response => {
// //           if (response.isSuccess && response.data) {
// //             return this.mapProductDTOToProduct(response.data);
// //           }
// //           throw new Error(response.message || 'Failed to update product');
// //         })
// //       );
// //   }

// //   // DELETE /api/Products/{id} - Delete product
// //   deleteProduct(id: number): Observable<boolean> {
// //     return this.http.delete<ApiResult<any>>(`${this.apiUrl}/${id}`)
// //       .pipe(
// //         tap(response => {
// //           if (response.isSuccess) {
// //             const currentProducts = this.productsSubject.value;
// //             this.productsSubject.next(currentProducts.filter(p => p.id !== id));
// //           }
// //         }),
// //         catchError(this.handleError),
// //         map(response => {
// //           if (response.isSuccess) {
// //             return true;
// //           }
// //           throw new Error(response.message || 'Failed to delete product');
// //         })
// //       );
// //   }

// //   // POST /api/Products/{id}/image - Upload product image
// //   uploadProductImage(productId: number, imageFile: File): Observable<string> {
// //     const formData = new FormData();
// //     formData.append('file', imageFile);

// //     return this.http.post<ApiResult<string>>(`${this.apiUrl}/${productId}/image`, formData)
// //       .pipe(
// //         catchError(this.handleError),
// //         map(response => {
// //           if (response.isSuccess && response.data) {
// //             // Update the product in local state with new image path
// //             const currentProducts = this.productsSubject.value;
// //             const productIndex = currentProducts.findIndex(p => p.id === productId);
// //             if (productIndex !== -1) {
// //               currentProducts[productIndex].image = response.data;
// //               this.productsSubject.next([...currentProducts]);
// //             }
// //             return response.data;
// //           }
// //           throw new Error(response.message || 'Failed to upload image');
// //         })
// //       );
// //   }

// //   // Load categories (assuming there's a categories endpoint)
// //   private loadCategories(): void {
// //     // Get unique categories from current products or from a dedicated endpoint
// //     this.products$.subscribe(products => {
// //       const categories = [...new Set(products.map(p => p.category))].sort();
// //       this.categoriesSubject.next(categories);
// //     });
// //   }

// //   // Get categories - public method
// //   getCategories(): Observable<string[]> {
// //     // If categories are empty, try to load them from products
// //     if (this.categoriesSubject.value.length === 0) {
// //       // Load first page to get some categories
// //       this.getAllProducts(1, 100).subscribe();
// //     }
// //     return this.categories$;
// //   }

// //   // Search products
// //   searchProducts(searchTerm: string, page: number = 1, pageSize: number = 12): Observable<ProductResponse> {
// //     const filter: ProductFilter = { searchTerm };
// //     return this.getAllProducts(page, pageSize, filter);
// //   }

// //   // Get products by category with pagination
// //   getProductsByCategoryPaginated(category: string, page: number = 1, pageSize: number = 12): Observable<ProductResponse> {
// //     const filter: ProductFilter = { category };
// //     return this.getAllProducts(page, pageSize, filter);
// //   }

// //   // Utility methods for state management
// //   clearProducts(): void {
// //     this.productsSubject.next([]);
// //     this.currentPageSubject.next(1);
// //     this.totalPagesSubject.next(0);
// //     this.totalCountSubject.next(0);
// //   }

// //   refreshProducts(): void {
// //     const currentPage = this.currentPageSubject.value;
// //     this.getAllProducts(currentPage).subscribe();
// //   }

// //   // Mapping functions
// //   private mapProductDTOToProduct(dto: ProductDTO): Product {
// //     return {
// //       id: dto.productId,
// //       productCode: dto.productCode,
// //       name: dto.name,
// //       category: dto.category,
// //       image: dto.imagePath,
// //       price: dto.price,
// //       minimumQuantity: dto.minimumQuantity,
// //       discountRate: dto.discountRate,
// //       createdAt: new Date(dto.createdAt),
// //       updatedAt: new Date(dto.updatedAt)
// //     };
// //   }

// //   private mapProductDTOsToProducts(dtos: ProductDTO[]): Product[] {
// //     return dtos.map(dto => this.mapProductDTOToProduct(dto));
// //   }

// //   // Error handling
// //   private handleError(error: any): Observable<never> {
// //     let errorMessage = 'An error occurred while processing your request';

// //     if (error.error instanceof ErrorEvent) {
// //       // Client-side error
// //       errorMessage = error.error.message;
// //     } else if (error.status) {
// //       // Server-side error
// //       switch (error.status) {
// //         case 400:
// //           errorMessage = error.error?.message || 'Invalid request data';
// //           break;
// //         case 401:
// //           errorMessage = 'Unauthorized access - Please login again';
// //           break;
// //         case 403:
// //           errorMessage = 'Access forbidden - Insufficient permissions';
// //           break;
// //         case 404:
// //           errorMessage = 'Resource not found';
// //           break;
// //         case 409:
// //           errorMessage = 'Resource already exists';
// //           break;
// //         case 422:
// //           errorMessage = error.error?.message || 'Invalid data provided';
// //           break;
// //         case 500:
// //           errorMessage = 'Server error occurred';
// //           break;
// //         case 503:
// //           errorMessage = 'Service temporarily unavailable';
// //           break;
// //         default:
// //           errorMessage = error.error?.message || `Error Code: ${error.status}`;
// //       }
// //     } else if (error.message) {
// //       errorMessage = error.message;
// //     }

// //     console.error('Product service error:', error);
// //     return throwError(() => new Error(errorMessage));
// //   }

// //   // Get current state values
// //   get currentProducts(): Product[] {
// //     return this.productsSubject.value;
// //   }

// //   get isLoading(): boolean {
// //     return this.loadingSubject.value;
// //   }

// //   get currentCategories(): string[] {
// //     return this.categoriesSubject.value;
// //   }
// // }
// }
