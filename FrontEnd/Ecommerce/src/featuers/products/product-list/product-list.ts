import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil, Subscription } from 'rxjs';
import { ProductService } from '../../../services/product-services';
import { AuthService } from '../../../services/auth-service';
import { Product } from '../../../models/Products';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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

  // Search functionality
  searchCategory: string = '';

  // Modal states
  showUpdateModal: boolean = false;
  showCreateModal:boolean=false;
  showDetailsModal: boolean = false;
  showDeleteModal: boolean = false;
  selectedProduct: Product = {} as Product;
  productToDelete: Product | null = null;

  // Loading states for operations
  isUpdating: boolean = false;
  isDeleting: boolean = false;

  // Hover functionality (keeping for backward compatibility)
  hoveredProduct: Product | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 12;
  totalPages = 1;
  totalProducts = 0;

  // View mode
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

    this.productService.getAllProducts()
      .subscribe({
        next: (response) => {
          this.products = response.items || [];
          this.filteredProducts = [...this.products]; // Initialize filtered products
          this.totalProducts = this.filteredProducts.length;
          this.calculatePagination();
          this.updatePaginatedProducts();
          console.log('Products loaded:', response);
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.message;
          this.isLoading = false;
        }
      });
  }
   public CreateProduct(prd:Product): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.productService.createProduct(prd)
      .subscribe({
        next: (response) => {

          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.message;
          this.isLoading = false;
        }
      });
  }


  // Search functionality
  onSearchChange(): void {
    if (!this.searchCategory.trim()) {
      this.filteredProducts = [...this.products];
    } else {
      const searchTerm = this.searchCategory.toLowerCase().trim();
      this.filteredProducts = this.products.filter(product =>
        product.category?.toLowerCase().includes(searchTerm)
      );
    }

    this.totalProducts = this.filteredProducts.length;
    this.currentPage = 1; // Reset to first page
    this.calculatePagination();
    this.updatePaginatedProducts();
  }

  clearSearch(): void {
    this.searchCategory = '';
    this.filteredProducts = [...this.products];
    this.totalProducts = this.filteredProducts.length;
    this.currentPage = 1;
    this.calculatePagination();
    this.updatePaginatedProducts();
  }

  // Modal management
  openUpdateModal(product: Product): void {
    this.selectedProduct = { ...product }; // Create a copy to avoid direct mutation
    this.showUpdateModal = true;
    document.body.style.overflow = 'hidden'; // Prevent background scrolling
  }
  openCreateModal( ): void {
    var prd :Product
    this.showCreateModal = true;

    document.body.style.overflow = 'hidden'; // Prevent background scrolling
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
    this.selectedProduct = {} as Product;
    this.isUpdating = false;
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

  // CRUD operations
  updateProduct(): void {
    if (!this.selectedProduct.id) {
      this.showErrorMessage('Product ID is required for update');
      return;
    }

    this.isUpdating = true;

    this.productService.updateProduct(this.selectedProduct.id, this.selectedProduct)
      .subscribe({
        next: (updatedProduct) => {
          // Update the product in the arrays
          const index = this.products.findIndex(p => p.id === updatedProduct.id);
          if (index !== -1) {
            this.products[index] = updatedProduct;
            this.onSearchChange(); // Refresh filtered products
          }

          this.showSuccessMessage('Product updated successfully!');
          this.closeUpdateModal();
        },
        error: (error) => {
          this.showErrorMessage('Failed to update product: ' + error.message);
          this.isUpdating = false;
        }
      });
  }

  deleteProduct(): void {
    if (!this.productToDelete?.id) {
      this.showErrorMessage('Product ID is required for deletion');
      return;
    }

    this.isDeleting = true;

    this.productService.deleteProduct(this.productToDelete.id)
      .subscribe({
        next: () => {
          // Remove the product from arrays
          this.products = this.products.filter(p => p.id !== this.productToDelete!.id);
          this.onSearchChange(); // Refresh filtered products

          this.showSuccessMessage('Product deleted successfully!');
          this.closeDeleteModal();
        },
        error: (error) => {
          this.showErrorMessage('Failed to delete product: ' + error.message);
          this.isDeleting = false;
        }
      });
  }

  private calculatePagination(): void {
    this.totalPages = Math.ceil(this.totalProducts / this.pageSize);
  }

  private updatePaginatedProducts(): void {
    if (!this.filteredProducts || this.filteredProducts.length === 0) {
      this.paginatedProducts = [];
      return;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;

    this.paginatedProducts = this.filteredProducts.slice(startIndex, endIndex);
    console.log("updatePaginatedProducts: ", this.paginatedProducts);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePaginatedProducts();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  getPaginationArray(): number[] {
    const pages: number[] = [];
    const startPage = Math.max(1, this.currentPage - 2);
    const endPage = Math.min(this.totalPages, this.currentPage + 2);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
  }

  // Existing hover functionality (keeping for backward compatibility)
  showProductDetails(product: Product): void {
    this.hoveredProduct = product;
  }

  hideProductDetails(): void {
    this.hoveredProduct = null;
  }

  getStockClass(stock: number): string {
    if (stock > 50) return 'stock-high';
    if (stock > 10) return 'stock-medium';
    return 'stock-low';
  }

  addToCart(product: Product): void {
    event?.stopPropagation();
    console.log('Adding to cart:', product);
    this.hideProductDetails();
  }

  viewFullDetails(product: Product): void {
    event?.stopPropagation();
    this.router.navigate(['/product', product.id]);
  }

  onProductClick(product: Product): void {
    this.router.navigate(['/product', product.id]);
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

  get currentUser() {
    return this.authService.currentUserValue;
  }

  getProductImageUrl(product: Product): string {
    return product.image || '/assets/images/1.png';
  }

  onImageError(event: any): void {
    event.target.src = '/assets/images/1.png';
  }

  private showSuccessMessage(message: string): void {
    // You can replace this with your notification service
    console.log('Success:', message);
    // Example with a simple alert (replace with toast/snackbar)
    alert(message);
  }

  private showErrorMessage(message: string): void {
    // You can replace this with your notification service
    console.error('Error:', message);
    // Example with a simple alert (replace with toast/snackbar)
    alert(message);
  }

  formatWeight(weight: string | number): string {
    if (!weight) return '';
    return typeof weight === 'string' ? weight : `${weight} kg`;
  }

  formatDimensions(dimensions: string): string {
    if (!dimensions) return '';
    return dimensions;
  }

  isInStock(product: Product): boolean {
    return product.minimumQuantity > 0;
  }

  getStockStatus(stock: number): string {
    if (stock === 0) return 'Out of Stock';
    if (stock <= 5) return 'Low Stock';
    if (stock <= 20) return 'Limited Stock';
    return 'In Stock';
  }

  onProductKeyDown(event: KeyboardEvent, product: Product): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.onProductClick(product);
    }
  }

  // Handle ESC key to close modals
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      if (this.showUpdateModal) this.closeUpdateModal();
      if (this.showDetailsModal) this.closeDetailsModal();
      if (this.showDeleteModal) this.closeDeleteModal();
    }
  }
}
