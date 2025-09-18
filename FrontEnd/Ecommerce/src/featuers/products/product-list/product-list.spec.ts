import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProductList } from '../../../featuers/products/product-list/product-list';
import { ProductService } from '../../../services/product-service';
import { AuthService } from '../../../services/auth-service';
import { Product, ProductResponse } from '../../../models/Products';

describe('ProductListComponent', () => {
  let component: ProductList;
  let fixture: ComponentFixture<ProductList>;
  let productService: jest.Mocked<ProductService>;
  let authService: jest.Mocked<AuthService>;
  let router: jest.Mocked<Router>;

  const mockProducts: Product[] = [
    { id: 1, productCode: 'P001', name: 'Test Product 1', category: 'Electronics', image: 'test1.jpg', price: 100, minimumQuantity: 1, discountRate: 10 },
    { id: 2, productCode: 'P002', name: 'Test Product 2', category: 'Clothing', image: 'test2.jpg', price: 50, minimumQuantity: 2, discountRate: 0 }
  ];

  const mockProductResponse: ProductResponse = {
    products: mockProducts,
    totalCount: 2,
    pageSize: 12,
    currentPage: 1,
    totalPages: 1
  };

  const mockUser = { id: 1, username: 'testuser', email: 'test@example.com' };

  beforeEach(async () => {
    const productServiceMock: Partial<jest.Mocked<ProductService>> = {
      getAllProducts: jest.fn(),
      getCategories: jest.fn()
    };
    const authServiceMock: Partial<jest.Mocked<AuthService>> = {
      logout: jest.fn(),
      currentUserValue: mockUser
    };
    const routerMock: Partial<jest.Mocked<Router>> = {
      navigate: jest.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [ProductList],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: ProductService, useValue: productServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductList);
    component = fixture.componentInstance;
    productService = TestBed.inject(ProductService) as jest.Mocked<ProductService>;
    authService = TestBed.inject(AuthService) as jest.Mocked<AuthService>;
    router = TestBed.inject(Router) as jest.Mocked<Router>;
  });

  beforeEach(() => {
    productService.getAllProducts.mockReturnValue(of(mockProductResponse));
    productService.getCategories.mockReturnValue(of(['Electronics', 'Clothing']));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load products on init', () => {
    expect(productService.getAllProducts).toHaveBeenCalled();
    expect(component.products).toEqual(mockProducts);
    expect(component.filteredProducts).toEqual(mockProducts);
    expect(component.totalProducts).toBe(2);
  });

  it('should load categories on init', () => {
    expect(productService.getCategories).toHaveBeenCalled();
    expect(component.categories).toEqual(['Electronics', 'Clothing']);
  });

  it('should apply filters when form changes', () => {
    component.searchForm.patchValue({ searchTerm: 'Test Product 1' });
    expect(productService.getAllProducts).toHaveBeenCalledWith(1, 12, { searchTerm: 'Test Product 1' });
  });

  it('should clear filters', () => {
    component.searchForm.patchValue({ searchTerm: 'test', category: 'Electronics' });
    component.clearFilters();

    expect(component.searchForm.get('searchTerm')?.value).toBeNull();
    expect(component.searchForm.get('category')?.value).toBeNull();
    expect(component.currentPage).toBe(1);
  });

  it('should change pages', () => {
    component.onPageChange(2);
    expect(component.currentPage).toBe(2);
    expect(productService.getAllProducts).toHaveBeenCalledWith(2, 12, {});
  });

  it('should not change to invalid page', () => {
    component.totalPages = 1;
    component.currentPage = 1;
    component.onPageChange(2);
    expect(component.currentPage).toBe(1);
  });

  it('should toggle view mode', () => {
    expect(component.viewMode).toBe('grid');
    component.toggleViewMode();
    expect(component.viewMode).toBe('list');
    component.toggleViewMode();
    expect(component.viewMode).toBe('grid');
  });

  it('should navigate to product detail on click', () => {
    component.onProductClick(mockProducts[0]);
    expect(router.navigate).toHaveBeenCalledWith(['/products', 1]);
  });

  it('should calculate discounted price correctly', () => {
    expect(component.getDiscountedPrice(mockProducts[0])).toBe(90);
  });

  it('should check if product has discount', () => {
    expect(component.hasDiscount(mockProducts[0])).toBeTruthy();
    expect(component.hasDiscount(mockProducts[1])).toBeFalsy();
  });

  it('should format price correctly', () => {
    expect(component.formatPrice(100)).toBe('$100.00');
  });

  it('should logout user', () => {
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should handle product loading error', () => {
    const errorMessage = 'Failed to load products';
    productService.getAllProducts.mockReturnValue(throwError(() => new Error(errorMessage)));

    component.loadProducts();

    expect(component.errorMessage).toBe(errorMessage);
    expect(component.isLoading).toBe(false);
  });

  it('should generate pagination array correctly', () => {
    component.currentPage = 3;
    component.totalPages = 10;
    expect(component.getPaginationArray()).toEqual([1, 2, 3, 4, 5]);
  });

  it('should handle image error', () => {
    const event: any = { target: { src: 'invalid-image.jpg' } };
    component.onImageError(event);
    expect(event.target.src).toBe('/assets/images/product-placeholder.jpg');
  });

  it('should get correct product image URL', () => {
    expect(component.getProductImageUrl(mockProducts[0])).toBe('test1.jpg');
    expect(component.getProductImageUrl({ ...mockProducts[1], image: undefined })).toBe('/assets/images/product-placeholder.jpg');
  });
});
