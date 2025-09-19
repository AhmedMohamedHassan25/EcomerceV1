describe('Login and Product Flow', () => {
  const baseUrl = Cypress.config('baseUrl') || 'http://localhost:4200';

  beforeEach(() => {
    // Mock API responses
    cy.intercept('POST', '/api/auth/login', {
      statusCode: 200,
      body: {
        token: 'mock-jwt-token',
        refreshToken: 'mock-refresh-token',
        user: {
          id: 1,
          userName: 'testuser',
          email: 'test@example.com'
        },
        expiresAt: new Date(Date.now() + 3600000).toISOString()
      }
    }).as('loginRequest');

    cy.intercept('GET', '/api/products*', {
      statusCode: 200,
      body: {
        products: [
          {
            productId: 1,
            productCode: 'P001',
            name: 'Test Product 1',
            category: 'Electronics',
            imagePath: 'test1.jpg',
            price: 100,
            minimumQuantity: 1,
            discountRate: 10
          },
          {
            productId: 2,
            productCode: 'P002',
            name: 'Test Product 2',
            category: 'Clothing',
            price: 50,
            minimumQuantity: 2,
            discountRate: 0
          }
        ],
        totalCount: 2,
        pageSize: 12,
        currentPage: 1,
        totalPages: 1
      }
    }).as('productsRequest');

    cy.intercept('GET', '/api/products/categories', {
      statusCode: 200,
      body: ['Electronics', 'Clothing', 'Books']
    }).as('categoriesRequest');
  });

  describe('Authentication Flow', () => {
    it('should redirect unauthenticated users to login', () => {
      cy.visit('/products');
      cy.url().should('include', '/login');
    });

    it('should display login form correctly', () => {
      cy.visit('/login');

      cy.get('h2').should('contain', 'Welcome Back');
      cy.get('input[formControlName="username"]').should('be.visible');
      cy.get('input[formControlName="password"]').should('be.visible');
      cy.get('button[type="submit"]').should('contain', 'Sign In');
    });

    it('should show validation errors for empty fields', () => {
      cy.visit('/login');

      cy.get('input[formControlName="username"]').focus().blur();
      cy.get('input[formControlName="password"]').focus().blur();

      cy.get('.field-error').should('contain', 'Username is required');
      cy.get('.field-error').should('contain', 'Password is required');
    });

    it('should successfully login with valid credentials', () => {
      cy.visit('/login');

      cy.get('input[formControlName="username"]').type('testuser');
      cy.get('input[formControlName="password"]').type('password123');
      cy.get('button[type="submit"]').click();

      cy.wait('@loginRequest');
      cy.url().should('include', '/products');
    });

    it('should toggle password visibility', () => {
      cy.visit('/login');

      cy.get('input[formControlName="password"]').should('have.attr', 'type', 'password');
      cy.get('.password-toggle').click();
      cy.get('input[formControlName="password"]').should('have.attr', 'type', 'text');
    });
  });

  describe('Product List Flow', () => {
    beforeEach(() => {
      // Set up authenticated state
      cy.window().then((win:Window) => {
        win.localStorage.setItem('access_token', 'mock-jwt-token');
        win.localStorage.setItem('refresh_token', 'mock-refresh-token');
        win.localStorage.setItem('current_user', JSON.stringify({
          id: 1,
          username: 'testuser',
          email: 'test@example.com'
        }));
      });
    });

    it('should display product list correctly', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');
      cy.wait('@categoriesRequest');

      cy.get('.product-card').should('have.length', 2);
      cy.get('h1').should('contain', 'Products');
      cy.get('.results-count').should('contain', '2 products found');
    });

    it('should display user info and logout button', () => {
      cy.visit('/products');

      cy.get('.user-info span').should('contain', 'Welcome, testuser');
      cy.get('.logout-btn').should('be.visible');
    });

    it('should filter products by search term', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('input[formControlName="searchTerm"]').type('Test Product 1');

      cy.wait('@productsRequest');
      cy.get('@productsRequest').should((interception) => {
        expect(interception.request.url).to.include('searchTerm=Test Product 1');
      });
    });

    it('should filter products by category', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('select[formControlName="category"]').select('Electronics');

      cy.wait('@productsRequest');
      cy.get('@productsRequest').should((interception) => {
        expect(interception.request.url).to.include('category=Electronics');
      });
    });

    it('should filter products by price range', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('input[formControlName="minPrice"]').type('10');
      cy.get('input[formControlName="maxPrice"]').type('100');

      cy.wait('@productsRequest');
      cy.get('@productsRequest').should((interception) => {
        expect(interception.request.url).to.include('minPrice=10');
        expect(interception.request.url).to.include('maxPrice=100');
      });
    });

    it('should clear all filters', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('input[formControlName="searchTerm"]').type('test');
      cy.get('select[formControlName="category"]').select('Electronics');
      cy.get('.clear-btn').click();

      cy.get('input[formControlName="searchTerm"]').should('have.value', '');
      cy.get('select[formControlName="category"]').should('have.value', '');
    });

    it('should toggle between grid and list view', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('.products-container').should('have.class', 'grid-view');
      cy.get('.view-toggle').click();
      cy.get('.products-container').should('have.class', 'list-view');
    });

    it('should display product information correctly', () => {
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('.product-card').first().within(() => {
        cy.get('.product-name').should('contain', 'Test Product 1');
        cy.get('.product-code').should('contain', 'P001');
        cy.get('.product-category').should('contain', 'Electronics');
        cy.get('.current-price').should('contain', '$90.00'); // With 10% discount
        cy.get('.discount-badge').should('contain', '-10%');
      });
    });

    it('should handle empty search results', () => {
      cy.intercept('GET', '/api/products*', {
        statusCode: 200,
        body: {
          products: [],
          totalCount: 0,
          pageSize: 12,
          currentPage: 1,
          totalPages: 0
        }
      }).as('emptyProductsRequest');

      cy.visit('/products');
      cy.wait('@emptyProductsRequest');

      cy.get('.empty-state').should('be.visible');
      cy.get('.empty-state h3').should('contain', 'No products found');
    });

    it('should logout successfully', () => {
      cy.visit('/products');

      cy.get('.logout-btn').click();
      cy.url().should('include', '/login');

      // Verify tokens are cleared
      cy.window().then((win:Window) => {
        expect(win.localStorage.getItem('access_token')).to.be.null;
        expect(win.localStorage.getItem('current_user')).to.be.null;
      });
    });
  });

  describe('Error Handling', () => {
    it('should handle login errors', () => {
      cy.intercept('POST', '/api/auth/login', {
        statusCode: 401,
        body: { message: 'Invalid credentials' }
      }).as('loginError');

      cy.visit('/login');

      cy.get('input[formControlName="username"]').type('wronguser');
      cy.get('input[formControlName="password"]').type('wrongpass');
      cy.get('button[type="submit"]').click();

      cy.wait('@loginError');
      cy.get('.error-message').should('contain', 'Invalid credentials');
    });

    it('should handle product loading errors', () => {
      cy.window().then((win:Window) => {
        win.localStorage.setItem('access_token', 'mock-jwt-token');
      });

      cy.intercept('GET', '/api/products*', {
        statusCode: 500,
        body: { message: 'Server error' }
      }).as('productsError');

      cy.visit('/products');
      cy.wait('@productsError');

      cy.get('.error-container').should('be.visible');
      cy.get('.error-message').should('contain', 'Server error');
      cy.get('.retry-btn').should('be.visible');
    });
  });

  describe('Responsive Design', () => {
    beforeEach(() => {
      cy.window().then((win:Window) => {
        win.localStorage.setItem('access_token', 'mock-jwt-token');
        win.localStorage.setItem('current_user', JSON.stringify({
          id: 1,
          username: 'testuser',
          email: 'test@example.com'
        }));
      });
    });

    it('should adapt layout for mobile devices', () => {
      cy.viewport(375, 667); // iPhone SE
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('.header-content').should('have.css', 'flex-direction', 'column');
      cy.get('.filters-form').should('have.css', 'flex-direction', 'column');
    });

    it('should maintain functionality on tablet', () => {
      cy.viewport(768, 1024); // iPad
      cy.visit('/products');
      cy.wait('@productsRequest');

      cy.get('.product-card').should('be.visible');
      cy.get('.search-input').type('test');
      cy.get('.view-toggle').click();
    });
  });
});
