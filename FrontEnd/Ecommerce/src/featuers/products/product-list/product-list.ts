import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil, Subscription } from 'rxjs';
import { environment } from '../../../enviroments/environment.development';
import { ProductService } from '../../../services/product-services';
import { AuthService } from '../../../services/auth-service';
import { Product } from '../../../models/Products';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../../models/User';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, FormsModule],
templateUrl: './product-list.html',
  styleUrls: ['./product-list.css']
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  paginatedProducts: Product[] = [];
  isLoading = false;
  errorMessage = '';
  sub: Subscription[] = [] as Subscription[];

  searchCategory: string = '';

  showUpdateModal: boolean = false;
  showCreateModal:boolean=false;
  showDetailsModal: boolean = false;
  showDeleteModal: boolean = false;
  selectedProduct: Product = {} as Product;
  SelectedCreatedProduct:Product={} as Product;
  productToDelete: Product  | null = null;

  showImageUpload  = false;
  selectedFile: File | null = null;

  isUpdating: boolean = false;
  isCreating: boolean = false;
  isDeleting: boolean = false;

  hoveredProduct: Product | null = null;

  currentPage = 1;
  pageSize = 12;
  totalPages = 1;
  totalProducts = 0;

  viewMode: 'grid' | 'list' = 'grid';

  private destroy$ = new Subject<void>();

  constructor(
    private productService: ProductService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    for (const element of this.sub) {
      element.unsubscribe();
    }
  }

  public loadProducts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    // استخدام الـ pagination parameters
    this.productService.getAllProducts(this.currentPage, this.pageSize)
      .subscribe({
        next: (response) => {
          this.products = response.items || [];
          this.paginatedProducts = [...this.products]; // المنتجات اللي جاية من الـ API مباشرة
          this.totalProducts = response.totalCount || 0; // إجمالي العدد من الـ response
          this.calculatePagination();

          // تطبيق الفلتر لو موجود
          this.applyCurrentFilter();

          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.message;
          this.isLoading = false;
        }
      });
  }

  private applyCurrentFilter(): void {
    if (!this.searchCategory.trim()) {
      this.filteredProducts = [...this.products];
      this.paginatedProducts = [...this.products];
    } else {
      const searchTerm = this.searchCategory.toLowerCase().trim();
      this.filteredProducts = this.products.filter(product =>
        product.category?.toLowerCase().includes(searchTerm)
      );
      this.paginatedProducts = [...this.filteredProducts];
    }
  }

  onSearchChange(): void {
    // إعادة تعيين الصفحة للأولى عند البحث
    this.currentPage = 1;

    if (!this.searchCategory.trim()) {
      // لو مفيش بحث، جيب البيانات من الـ API
      this.loadProducts();
    } else {
      // لو في بحث، فلتر البيانات المحلية
      const searchTerm = this.searchCategory.toLowerCase().trim();
      this.filteredProducts = this.products.filter(product =>
        product.category?.toLowerCase().includes(searchTerm)
      );
      this.paginatedProducts = [...this.filteredProducts];

      // حساب pagination للبيانات المفلترة محلياً
      this.totalProducts = this.filteredProducts.length;
      this.calculatePagination();
    }
  }

  clearSearch(): void {
    this.searchCategory = '';
    this.currentPage = 1;
    this.loadProducts(); // إعادة تحميل البيانات من الـ API
  }

  private calculatePagination(): void {
    if (this.searchCategory.trim()) {
      // لو في فلتر، احسب على أساس البيانات المفلترة
      this.totalPages = Math.ceil(this.filteredProducts.length / this.pageSize);
    } else {
      // لو مفيش فلتر، احسب على أساس إجمالي البيانات من الـ server
      this.totalPages = Math.ceil(this.totalProducts / this.pageSize);
    }

    if (this.currentPage > this.totalPages && this.totalPages > 0) {
      this.currentPage = this.totalPages;
    }
    if (this.currentPage < 1) {
      this.currentPage = 1;
    }
    console.log('Calculated pagination - Total Pages:', this.totalPages, 'Current Page:', this.currentPage);
  }

  onPageChange(page: number): void {
    console.log('Page change requested:', page, 'Current:', this.currentPage, 'Total:', this.totalPages);

    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;

      if (this.searchCategory.trim()) {
        // لو في فلتر، اعمل pagination محلي
        this.updatePaginatedProducts();
      } else {
        // لو مفيش فلتر، اجيب صفحة جديدة من الـ server
        this.loadProducts();
      }

      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  private updatePaginatedProducts(): void {
    if (!this.filteredProducts || this.filteredProducts.length === 0) {
      this.paginatedProducts = [];
      return;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;

    this.paginatedProducts = this.filteredProducts.slice(startIndex, endIndex);
    console.log('Updated paginated products:', this.paginatedProducts.length, 'Start:', startIndex, 'End:', endIndex);
  }

  getPaginationArray(): number[] {
    const pages: number[] = [];
    const startPage = Math.max(1, this.currentPage - 2);
    const endPage = Math.min(this.totalPages, this.currentPage + 2);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    console.log('Pagination array:', pages);
    return pages;
  }

  public CreateProduct(prd:Product): void {
    this.isLoading = true;
    this.errorMessage = '';
  }

  uploadProductImage(productId: number, file: File): void {
    this.isLoading = true;

    this.productService.updateProductImage(productId, file).subscribe({
      next: () => {
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Image upload failed", err);
        this.isLoading = false;
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  openUpdateModal(product: Product): void {
    this.selectedProduct = product as Product;
    this.showUpdateModal = true;
    document.body.style.overflow = 'hidden';
  }

  openCreateModal(): void {
    this.SelectedCreatedProduct = {} as Product;
    this.showCreateModal = true;
    document.body.style.overflow = 'hidden';
  }

  openDetailsModal(product: Product): void {
    this.selectedProduct = product;
    this.showDetailsModal = true;
    document.body.style.overflow = 'hidden';
  }

  confirmDelete(product: Product): void {
    this.productToDelete = product;
    this.showDeleteModal = true;
    document.body.style.overflow = 'hidden';
  }

  closeUpdateModal(): void {
    this.showUpdateModal = false;
    this.selectedProduct = {} as Product;
    this.isUpdating = false;
    document.body.style.overflow = 'auto';
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.SelectedCreatedProduct = {} as Product;
    this.isCreating = false;
    document.body.style.overflow = 'auto';
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedProduct = {} as Product;
    document.body.style.overflow = 'auto';
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.productToDelete = null;
    this.isDeleting = false;
    document.body.style.overflow = 'auto';
  }

  openUpdateModalFromDetails(): void {
    this.closeDetailsModal();
    setTimeout(() => {
      this.openUpdateModal(this.selectedProduct);
    }, 100);
  }

  updateProduct(): void {
    if (!this.selectedProduct.productId) {
      this.showErrorMessage('Product ID is required for update');
      return;
    }

    this.isUpdating = true;

    this.productService.updateProduct(this.selectedProduct.productId, this.selectedProduct)
      .subscribe({
        next: (updatedProduct) => {
          if (this.selectedFile) {
            this.uploadProductImage(this.selectedProduct.productId, this.selectedFile);
            this.showImageUpload = false;
            this.selectedFile = null;
          }

          // إعادة تحميل الصفحة الحالية
          this.loadProducts();

          this.showSuccessMessage('Product updated successfully!');
          this.closeUpdateModal();
        },
        error: (error) => {
          this.showErrorMessage('Failed to update product: ' + error.message);
          this.isUpdating = false;
        }
      });
  }

  createProduct(): void {
    this.isCreating = true;

    this.productService.createProduct(this.SelectedCreatedProduct)
      .subscribe({
        next: (response: any) => {
          let createdProduct: any;

          if (response.value) {
            createdProduct = response.value;
          } else if (response.data) {
            createdProduct = response.data;
          } else {
            createdProduct = response;
          }

          const productId = createdProduct.productId ||
                           createdProduct.id ||
                           createdProduct.ID ||
                           createdProduct.ProductId;

          if (!productId) {
            this.showErrorMessage('Product created but ID is missing');
            this.isCreating = false;
            return;
          }

          if (this.selectedFile) {
            this.uploadProductImage(productId, this.selectedFile);
            this.showImageUpload = false;
            this.selectedFile = null;
          }

          // إعادة تحميل الصفحة الحالية
          this.loadProducts();

          this.showSuccessMessage('Product created successfully!');
          this.closeCreateModal();
          this.isCreating = false;
        },
        error: (error) => {
          console.error('=== Product Creation Error ===');
          console.error(error);
          this.showErrorMessage('Failed to create product: ' + error.message);
          this.isCreating = false;
        }
      });
  }

  deleteProduct(): void {
    if (!this.productToDelete?.productId) {
      this.showErrorMessage('Product ID is required for deletion');
      return;
    }

    this.isDeleting = true;

    this.productService.deleteProduct(this.productToDelete.productId)
      .subscribe({
        next: () => {
          // إعادة تحميل الصفحة الحالية أو الرجوع للصفحة السابقة لو الصفحة الحالية فارغة
          this.loadProducts();

          this.showSuccessMessage('Product deleted successfully!');
          this.closeDeleteModal();
        },
        error: (error) => {
          this.showErrorMessage('Failed to delete product: ' + error.message);
          this.isDeleting = false;
        }
      });
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
  }

  showProductDetails(product: Product): void {
    this.hoveredProduct = product;
  }

  hideProductDetails(): void {
    this.hoveredProduct = null;
  }

  viewFullDetails(product: Product): void {
    event?.stopPropagation();
    this.router.navigate(['/product', product.productId]);
  }

  onProductClick(product: Product): void {
    this.router.navigate(['/product', product.productId]);
  }

  getDiscountedPrice(product: Product): number {
    return product.price * (1 - product.discountRate / 100);
  }

  hasDiscount(product: Product): boolean {
    return product.discountRate > 0;
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  public getcurrentUser(): User | null {
    return this.authService.currentUserValue;
  }

  getProductImageUrl(product: Product): string {
    return `${environment.baseImageUrl}/${product.imagePath}`;
  }

  onImageError(event: any): void {
    event.target.src = '/assets/images/1.png';
  }

  private showSuccessMessage(message: string): void {
    alert(message);
  }

  private showErrorMessage(message: string): void {
    console.error('Error:', message);
    alert(message);
  }

  onProductKeyDown(event: KeyboardEvent, product: Product): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.onProductClick(product);
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      if (this.showUpdateModal) this.closeUpdateModal();
      if (this.showCreateModal) this.closeCreateModal();
      if (this.showDetailsModal) this.closeDetailsModal();
      if (this.showDeleteModal) this.closeDeleteModal();
    }
  }
}
