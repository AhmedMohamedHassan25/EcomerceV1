import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProductListComponent } from './product-list';
import { ProductService } from '../../../services/product-services';
import { AuthService } from '../../../services/auth-service';
import { Product } from '../../../models/Products';
import { User } from '../../../models/User';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Mock window.scrollTo for Jest
Object.defineProperty(window, 'scrollTo', {
  value: jest.fn(),
  writable: true
});

// Mock console methods
global.console = {
  ...console,
  log: jest.fn(),
  error: jest.fn(),
};

// Mock window.alert
Object.defineProperty(window, 'alert', {
  value: jest.fn(),
  writable: true
});

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let mockProductService: jest.Mocked<ProductService>;
  let mockAuthService: jest.Mocked<AuthService>;
  let mockRouter: jest.Mocked<Router>;

  const mockProducts: Product[] = [
    {
      productId: 1,
      name: 'Test Product 1',
      productCode: 'TP001',
      category: 'Electronics',
      price: 100,
      minimumQuantity: 50,
      discountRate: 10,
      imagePath: 'test-image-1.jpg',
      createdAt: new Date('2024-01-01'),
      updatedAt: new Date('2024-01-01')
    },
    {
      productId: 2,
      name: 'Test Product 2',
      productCode: 'TP002',
      category: 'Books',
      price: 200,
      minimumQuantity: 0,
      discountRate: 0,
      imagePath: 'test-image-2.jpg',
      createdAt: new Date('2024-01-02'),
      updatedAt: new Date('2024-01-02')
    },
    {
      productId: 3,
      name: 'Test Product 3',
      productCode: 'TP003',
      category: 'Electronics',
      price: 150,
      minimumQuantity: 25,
      discountRate: 15,
      imagePath: 'test-image-3.jpg',
      createdAt: new Date('2024-01-03'),
      updatedAt: new Date('2024-01-03')
    }
  ];

  const mockUser: User = {
    userId: 1,
    userName: 'testuser',
    email: 'test@example.com',
  };

  beforeEach(async () => {
    const productServiceMock = {
      getAllProducts: jest.fn(),
      createProduct: jest.fn(),
      updateProduct: jest.fn(),
      deleteProduct: jest.fn()
    };

    const authServiceMock = {
      logout: jest.fn(),
      currentUserValue: mockUser
    };

    const routerMock = {
      navigate: jest.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ProductListComponent, CommonModule, FormsModule],
      providers: [
        { provide: ProductService, useValue: productServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    mockProductService = TestBed.inject(ProductService) as jest.Mocked<ProductService>;
    mockAuthService = TestBed.inject(AuthService) as jest.Mocked<AuthService>;
    mockRouter = TestBed.inject(Router) as jest.Mocked<Router>;

      mockProductService.getAllProducts.mockReturnValue(
  of({
    items: mockProducts,
    totalCount: mockProducts.length,
    pageSize: 10,
    currentPage: 1,
    totalPages: 1
  })
);  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should load products on init', () => {
      component.ngOnInit();

      expect(mockProductService.getAllProducts).toHaveBeenCalled();
      expect(component.products).toEqual(mockProducts);
      expect(component.filteredProducts).toEqual(mockProducts);
      expect(component.totalProducts).toBe(3);
      expect(component.isLoading).toBe(false);
    });

    it('should handle error when loading products', () => {
      const errorMessage = 'Failed to load products';
      mockProductService.getAllProducts.mockReturnValue(throwError({ message: errorMessage }));

      component.ngOnInit();

      expect(component.errorMessage).toBe(errorMessage);
      expect(component.isLoading).toBe(false);
      expect(component.products).toEqual([]);
    });
  });

  describe('Search Functionality', () => {
    beforeEach(() => {
      component.products = mockProducts;
      component.filteredProducts = mockProducts;
    });

    it('should filter products by category', () => {
      component.searchCategory = 'Electronics';
      component.onSearchChange();

      expect(component.filteredProducts.length).toBe(2);
      expect(component.filteredProducts.every(p => p.category === 'Electronics')).toBe(true);
      expect(component.currentPage).toBe(1);
    });

    it('should show all products when search is empty', () => {
      component.searchCategory = '';
      component.onSearchChange();

      expect(component.filteredProducts).toEqual(mockProducts);
      expect(component.totalProducts).toBe(3);
    });

    it('should clear search and reset filters', () => {
      component.searchCategory = 'Electronics';
      component.onSearchChange();

      component.clearSearch();

      expect(component.searchCategory).toBe('');
      expect(component.filteredProducts).toEqual(mockProducts);
      expect(component.currentPage).toBe(1);
    });
  });

  describe('Modal Operations', () => {
    // Mock document.body.style.overflow
    beforeEach(() => {
      Object.defineProperty(document.body.style, 'overflow', {
        set: jest.fn(),
        get: jest.fn(() => 'auto'),
        configurable: true
      });
    });

    it('should open update modal', () => {
      const product = mockProducts[0];

      component.openUpdateModal(product);

      expect(component.selectedProduct).toEqual(product);
      expect(component.showUpdateModal).toBe(true);
    });

    it('should close update modal', () => {
      component.showUpdateModal = true;
      component.selectedProduct = mockProducts[0];

      component.closeUpdateModal();

      expect(component.showUpdateModal).toBe(false);
      expect(component.selectedProduct).toEqual({} as Product);
      expect(component.isUpdating).toBe(false);
    });

    it('should open create modal', () => {
      component.openCreateModal();

      expect(component.SelectedCreatedProduct).toEqual({} as Product);
      expect(component.showCreateModal).toBe(true);
    });

    it('should close create modal', () => {
      component.showCreateModal = true;

      component.closeCreateModal();

      expect(component.showCreateModal).toBe(false);
      expect(component.SelectedCreatedProduct).toEqual({} as Product);
      expect(component.isCreating).toBe(false);
    });

    it('should open details modal', () => {
      const product = mockProducts[0];

      component.openDetailsModal(product);

      expect(component.selectedProduct).toEqual(product);
      expect(component.showDetailsModal).toBe(true);
    });

    it('should open delete confirmation modal', () => {
      const product = mockProducts[0];

      component.confirmDelete(product);

      expect(component.productToDelete).toEqual(product);
      expect(component.showDeleteModal).toBe(true);
    });
  });

  describe('CRUD Operations', () => {
    describe('Update Product', () => {
      it('should update product successfully', () => {
        const updatedProduct = { ...mockProducts[0], name: 'Updated Product' };
        component.products = mockProducts;
        component.selectedProduct = updatedProduct;
        mockProductService.updateProduct.mockReturnValue(of(updatedProduct));
        const showSuccessSpy = jest.spyOn(component, 'showSuccessMessage' as any);
        const closeModalSpy = jest.spyOn(component, 'closeUpdateModal');

        component.updateProduct();

        expect(mockProductService.updateProduct).toHaveBeenCalledWith(
          updatedProduct.productId,
          updatedProduct
        );
        expect(showSuccessSpy).toHaveBeenCalledWith('Product updated successfully!');
        expect(closeModalSpy).toHaveBeenCalled();
      });

      it('should handle update error', () => {
        const errorMessage = 'Update failed';
        component.selectedProduct = mockProducts[0];
        mockProductService.updateProduct.mockReturnValue(throwError({ message: errorMessage }));
        const showErrorSpy = jest.spyOn(component, 'showErrorMessage' as any);

        component.updateProduct();

        expect(showErrorSpy).toHaveBeenCalledWith('Failed to update product: ' + errorMessage);
        expect(component.isUpdating).toBe(false);
      });

      it('should show error when product ID is missing', () => {
        component.selectedProduct = { ...mockProducts[0], productId: 0 };
        const showErrorSpy = jest.spyOn(component, 'showErrorMessage' as any);

        component.updateProduct();

        expect(showErrorSpy).toHaveBeenCalledWith('Product ID is required for update');
      });
    });

    describe('Create Product', () => {
      it('should create product successfully', () => {
        const newProduct = { ...mockProducts[0], productId: 4, name: 'New Product' };
        component.products = mockProducts;
        component.SelectedCreatedProduct = newProduct;
        mockProductService.createProduct.mockReturnValue(of(newProduct));
        const showSuccessSpy = jest.spyOn(component, 'showSuccessMessage' as any);
        const closeModalSpy = jest.spyOn(component, 'closeCreateModal');
        const searchSpy = jest.spyOn(component, 'onSearchChange');

        component.createProduct();

        expect(mockProductService.createProduct).toHaveBeenCalledWith(newProduct);
        expect(component.products).toContain(newProduct);
        expect(showSuccessSpy).toHaveBeenCalledWith('Product created successfully!');
        expect(closeModalSpy).toHaveBeenCalled();
      });

      it('should handle create error', () => {
        const errorMessage = 'Create failed';
        component.SelectedCreatedProduct = mockProducts[0];
        mockProductService.createProduct.mockReturnValue(throwError({ message: errorMessage }));
        const showErrorSpy = jest.spyOn(component, 'showErrorMessage' as any);

        component.createProduct();

        expect(showErrorSpy).toHaveBeenCalledWith('Failed to create product: ' + errorMessage);
        expect(component.isCreating).toBe(false);
      });
    });

    describe('Delete Product', () => {
      it('should delete product successfully', () => {
        const productToDelete = mockProducts[0];
        component.products = [...mockProducts];
        component.productToDelete = productToDelete;
        mockProductService.deleteProduct.mockReturnValue(of());
        const showSuccessSpy = jest.spyOn(component, 'showSuccessMessage' as any);
        const closeModalSpy = jest.spyOn(component, 'closeDeleteModal');
        const searchSpy = jest.spyOn(component, 'onSearchChange');

        component.deleteProduct();

        expect(mockProductService.deleteProduct).toHaveBeenCalledWith(productToDelete.productId);
        expect(component.products).not.toContain(productToDelete);
        expect(showSuccessSpy).toHaveBeenCalledWith('Product deleted successfully!');
        expect(closeModalSpy).toHaveBeenCalled();
      });

      it('should handle delete error', () => {
        const errorMessage = 'Delete failed';
        component.productToDelete = mockProducts[0];
        mockProductService.deleteProduct.mockReturnValue(throwError({ message: errorMessage }));
        const showErrorSpy = jest.spyOn(component, 'showErrorMessage' as any);

        component.deleteProduct();

        expect(showErrorSpy).toHaveBeenCalledWith('Failed to delete product: ' + errorMessage);
        expect(component.isDeleting).toBe(false);
      });

      it('should show error when product ID is missing', () => {
        component.productToDelete = { ...mockProducts[0], productId: 0 };
        const showErrorSpy = jest.spyOn(component, 'showErrorMessage' as any);

        component.deleteProduct();

        expect(showErrorSpy).toHaveBeenCalledWith('Product ID is required for deletion');
      });
    });
  });

  describe('Pagination', () => {
    beforeEach(() => {
      component.filteredProducts = mockProducts;
      component.totalProducts = mockProducts.length;
      component.pageSize = 2;
    });

    it('should calculate pagination correctly', () => {
      component['calculatePagination']();

      expect(component.totalPages).toBe(2); // 3 products / 2 per page = 2 pages
    });

    it('should update paginated products', () => {
      component.currentPage = 1;
      component['updatePaginatedProducts']();

      expect(component.paginatedProducts.length).toBe(2);
      expect(component.paginatedProducts).toEqual([mockProducts[0], mockProducts[1]]);
    });

    it('should change page', () => {
      component.totalPages = 2;

      component.onPageChange(2);

      expect(component.currentPage).toBe(2);
      expect(window.scrollTo).toHaveBeenCalledWith({ top: 0, behavior: 'smooth' });
    });

    it('should not change to invalid page', () => {
      component.currentPage = 1;
      component.totalPages = 2;

      component.onPageChange(3);

      expect(component.currentPage).toBe(1);
    });

    it('should generate pagination array', () => {
      component.currentPage = 2;
      component.totalPages = 5;

      const pages = component.getPaginationArray();

      expect(pages).toEqual([1, 2, 3, 4]);
    });

    it('should handle empty filtered products', () => {
      component.filteredProducts = [];
      component['updatePaginatedProducts']();

      expect(component.paginatedProducts).toEqual([]);
    });
  });

  describe('View Mode', () => {
    it('should toggle view mode from grid to list', () => {
      component.viewMode = 'grid';

      component.toggleViewMode();

      expect(component.viewMode).toBe('list');
    });

    it('should toggle view mode from list to grid', () => {
      component.viewMode = 'list';

      component.toggleViewMode();

      expect(component.viewMode).toBe('grid');
    });
  });

  describe('Product Interaction', () => {
    it('should show product details on hover', () => {
      const product = mockProducts[0];

      component.showProductDetails(product);

      expect(component.hoveredProduct).toEqual(product);
    });

    it('should hide product details', () => {
      component.hoveredProduct = mockProducts[0];

      component.hideProductDetails();

      expect(component.hoveredProduct).toBeNull();
    });

    it('should navigate to product details on click', () => {
      const product = mockProducts[0];

      component.onProductClick(product);

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/product', product.productId]);
    });



    it('should handle view full details', () => {
      const product = mockProducts[0];

      component.viewFullDetails(product);

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/product', product.productId]);
    });
  });

  describe('Utility Methods', () => {
    it('should calculate discounted price', () => {
      const product = mockProducts[0]; // price: 100, discountRate: 10

      const discountedPrice = component.getDiscountedPrice(product);

      expect(discountedPrice).toBe(90);
    });

    it('should check if product has discount', () => {
      expect(component.hasDiscount(mockProducts[0])).toBe(true); // discountRate: 10
      expect(component.hasDiscount(mockProducts[1])).toBe(false); // discountRate: 0
    });

    it('should format price as currency', () => {
      const formatted = component.formatPrice(100.50);

      expect(formatted).toBe('$100.50');
    });







    it('should get product image URL with fallback', () => {
      const productWithImage = mockProducts[0];
      const productWithoutImage = { ...mockProducts[0], image: '' };

      expect(component.getProductImageUrl(productWithImage)).toBe(productWithImage.imagePath);
      expect(component.getProductImageUrl(productWithoutImage)).toBe('/assets/images/1.png');
    });
  });

  describe('Authentication', () => {
    it('should get current user', () => {
      const user = component.getcurrentUser();

      expect(user).toEqual(mockUser);
    });

    it('should logout user', () => {
      component.logout();

      expect(mockAuthService.logout).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('File Upload', () => {
    it('should handle file selection', () => {
      const mockFile = new File([''], 'test.jpg', { type: 'image/jpeg' });
      const mockEvent = {
        target: {
          files: [mockFile]
        }
      } as unknown as Event;

      component.onFileSelected(mockEvent);

      expect(component.selectedFile).toEqual(mockFile);
      expect(console.log).toHaveBeenCalledWith('Selected file:', mockFile);
    });

    it('should handle file selection with no files', () => {
      const mockEvent = {
        target: {
          files: null
        }
      } as unknown as Event;

      component.onFileSelected(mockEvent);

      expect(component.selectedFile).toBeNull();
    });
  });

  describe('Keyboard Navigation', () => {
    it('should handle product keyboard navigation with Enter', () => {
      const product = mockProducts[0];
      const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
      const preventDefaultSpy = jest.spyOn(enterEvent, 'preventDefault');
      const clickSpy = jest.spyOn(component, 'onProductClick');

      component.onProductKeyDown(enterEvent, product);

      expect(preventDefaultSpy).toHaveBeenCalled();
      expect(clickSpy).toHaveBeenCalledWith(product);
    });

    it('should handle product keyboard navigation with Space', () => {
      const product = mockProducts[0];
      const spaceEvent = new KeyboardEvent('keydown', { key: ' ' });
      const preventDefaultSpy = jest.spyOn(spaceEvent, 'preventDefault');
      const clickSpy = jest.spyOn(component, 'onProductClick');

      component.onProductKeyDown(spaceEvent, product);

      expect(preventDefaultSpy).toHaveBeenCalled();
      expect(clickSpy).toHaveBeenCalledWith(product);
    });

    it('should not handle other keys', () => {
      const product = mockProducts[0];
      const tabEvent = new KeyboardEvent('keydown', { key: 'Tab' });
      const preventDefaultSpy = jest.spyOn(tabEvent, 'preventDefault');
      const clickSpy = jest.spyOn(component, 'onProductClick');

      component.onProductKeyDown(tabEvent, product);

      expect(preventDefaultSpy).not.toHaveBeenCalled();
      expect(clickSpy).not.toHaveBeenCalled();
    });

    it('should close modals on ESC key', () => {
      const escEvent = new KeyboardEvent('keydown', { key: 'Escape' });

      // Test update modal
      component.showUpdateModal = true;
      const closeUpdateSpy = jest.spyOn(component, 'closeUpdateModal');
      component.onKeyDown(escEvent);
      expect(closeUpdateSpy).toHaveBeenCalled();

      // Test create modal
      component.showCreateModal = true;
      const closeCreateSpy = jest.spyOn(component, 'closeCreateModal');
      component.onKeyDown(escEvent);
      expect(closeCreateSpy).toHaveBeenCalled();

      // Test details modal
      component.showDetailsModal = true;
      const closeDetailsSpy = jest.spyOn(component, 'closeDetailsModal');
      component.onKeyDown(escEvent);
      expect(closeDetailsSpy).toHaveBeenCalled();

      // Test delete modal
      component.showDeleteModal = true;
      const closeDeleteSpy = jest.spyOn(component, 'closeDeleteModal');
      component.onKeyDown(escEvent);
      expect(closeDeleteSpy).toHaveBeenCalled();
    });
  });

  describe('Modal Transitions', () => {
    it('should transition from details to update modal', (done) => {
      component.selectedProduct = mockProducts[0];
      const closeDetailsSpy = jest.spyOn(component, 'closeDetailsModal');
      const openUpdateSpy = jest.spyOn(component, 'openUpdateModal');

      component.openUpdateModalFromDetails();

      expect(closeDetailsSpy).toHaveBeenCalled();

      // Wait for setTimeout
      setTimeout(() => {
        expect(openUpdateSpy).toHaveBeenCalledWith(mockProducts[0]);
        done();
      }, 150);
    });
  });

  describe('Component Cleanup', () => {
    it('should cleanup on destroy', () => {
      const mockSubscription = {
        unsubscribe: jest.fn()
      };
      component.sub = [mockSubscription as any];
      const destroyNextSpy = jest.spyOn(component['destroy$'], 'next');
      const destroyCompleteSpy = jest.spyOn(component['destroy$'], 'complete');

      component.ngOnDestroy();

      expect(destroyNextSpy).toHaveBeenCalled();
      expect(destroyCompleteSpy).toHaveBeenCalled();
      expect(mockSubscription.unsubscribe).toHaveBeenCalled();
    });
  });

  describe('Error Handling', () => {
    it('should handle image error', () => {
      const mockEvent = {
        target: {
          src: 'original-image.jpg'
        }
      };

      component.onImageError(mockEvent);

      expect(mockEvent.target.src).toBe('/assets/images/1.png');
    });

    it('should show success message', () => {
      const message = 'Success message';

      component['showSuccessMessage'](message);

      expect(console.log).toHaveBeenCalledWith('Success:', message);
      expect(window.alert).toHaveBeenCalledWith(message);
    });

    it('should show error message', () => {
      const message = 'Error message';

      component['showErrorMessage'](message);

      expect(console.error).toHaveBeenCalledWith('Error:', message);
      expect(window.alert).toHaveBeenCalledWith(message);
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty products array', () => {
      component.products = [];
      component.filteredProducts = [];
      component.onSearchChange();

      expect(component.totalProducts).toBe(0);
      expect(component.filteredProducts).toEqual([]);
    });

    it('should handle search with whitespace only', () => {
      component.products = mockProducts;
      component.searchCategory = '   ';
      component.onSearchChange();

      expect(component.filteredProducts).toEqual(mockProducts);
    });

    it('should handle case insensitive search', () => {
      component.products = mockProducts;
      component.searchCategory = 'electronics'; // lowercase
      component.onSearchChange();

      expect(component.filteredProducts.length).toBe(2);
      expect(component.filteredProducts.every(p => p.category?.toLowerCase().includes('electronics'))).toBe(true);
    });


  });
});
