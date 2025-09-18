import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProductList } from '../../../featuers/products/product-list/product-list';
import { ProdutService } from '../../../services/product-service';
import { AuthService } from '../../../services/auth-service';
import { Product, ProductResponse } from '../../../models/Products';



describe('ProductListComponent', () => {
  let component: ProductList;
  let fixture: ComponentFixture<ProductList>;
  let productService: jasmine.SpyObj<ProdutService>;
  let authService: jasmine.SpyObj<AuthService
  >;
  let router: jasmine.SpyObj<Router>;

  const mockProducts: Product[] = [
    {
      id: 1,
      productCode: 'P001',
      name: 'Test Product 1',
      category: 'Electronics',
      image: 'test1.jpg',
      price: 100,
      minimumQuantity: 1,
      discountRate: 10
    },
    {
      id: 2,
      productCode: 'P002',
      name: 'Test Product 2',
      category: 'Clothing',
      image: 'test2.jpg',
      price: 50,
      minimumQuantity: 2,
      discountRate: 0
    }
  ];

  const mockProductResponse: ProductResponse = {
    products: mockProducts,
    totalCount: 2,
    pageSize: 12,
    currentPage: 1,
    totalPages: 1
  };

  const mockUser = {
    id: 1,
    username: 'testuser',
    email: 'test@example.com'
  };

  beforeEach(async () => {
    const productServiceSpy = jasmine.createSpyObj('ProductService', [
      'getAllProducts',
      'getCategories'
    ]);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['logout'], {
      currentUserValue: mockUser
    });
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [ProductList],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: ProdutService, useValue: productServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductList);
    component = fixture.componentInstance;
    productService = TestBed.inject(ProdutService) as jasmine.SpyObj<ProdutService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  beforeEach(() => {
    productService.getAllProducts.and.returnValue(of(mockProductResponse));
    productService.getCategories.and.returnValue(of(['Electronics', 'Clothing']));
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
    component.searchForm.patchValue({
      searchTerm: 'Test Product 1'
    });

    expect(productService.getAllProducts).toHaveBeenCalledWith(
      1,
      12,
      { searchTerm: 'Test Product 1' }
    );
  });

  it('should clear filters', () => {
    component.searchForm.patchValue({
      searchTerm: 'test',
      category: 'Electronics'
    });

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
    const product = mockProducts[0];

    component.onProductClick(product);

    expect(router.navigate).toHaveBeenCalledWith(['/products', 1]);
  });

  it('should calculate discounted price correctly', () => {
    const product = mockProducts[0]; // 10% discount
    const discountedPrice = component.getDiscountedPrice(product);

    expect(discountedPrice).toBe(90); // 100 * (1 - 10/100)
  });

  it('should check if product has discount', () => {
    expect(component.hasDiscount(mockProducts[0])).toBeTruthy(); // 10% discount
    expect(component.hasDiscount(mockProducts[1])).toBeFalsy(); // 0% discount
  });

  it('should format price correctly', () => {
    const formattedPrice = component.formatPrice(100);
    expect(formattedPrice).toBe('$100.00');
  });

  it('should logout user', () => {
    component.logout();

    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should handle product loading error', () => {
    const errorMessage = 'Failed to load products';
    productService.getAllProducts.and.returnValue(throwError(() => new Error(errorMessage)));

    component.loadProducts();

    expect(component.errorMessage).toBe(errorMessage);
    expect(component.isLoading).toBeFalse();
  });

  it('should generate pagination array correctly', () => {
    component.currentPage = 3;
    component.totalPages = 10;

    const paginationArray = component.getPaginationArray();

    expect(paginationArray).toEqual([1, 2, 3, 4, 5]);
  });

  it('should handle image error', () => {
    const event = {
      target: {
        src: 'invalid-image.jpg'
      }
    };

    component.onImageError(event);

    expect(event.target.src).toBe('/assets/images/product-placeholder.jpg');
  });

  it('should get correct product image URL', () => {
    const productWithImage = mockProducts[0];
    const productWithoutImage = { ...mockProducts[1], image: undefined };

    expect(component.getProductImageUrl(productWithImage)).toBe('test1.jpg');
    expect(component.getProductImageUrl(productWithoutImage)).toBe('/assets/images/product-placeholder.jpg');
  });
});
