using Application.Contract;
using Application.Services.FileServices;
using Application.Services.ProductServices;
using DTOs.Product;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using System.Linq.Expressions;

namespace EcommerceSiteTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<ILogger<Product>> _mockLogger;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockFileService = new Mock<IFileService>();
            _mockLogger = new Mock<ILogger<Product>>();
            _productService = new ProductService(_mockProductRepository.Object, _mockFileService.Object, _mockLogger.Object);
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsProductDTO()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Category = "Electronics",
                Price = 100,
                ProductCode = "P001"
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal("Test Product", result.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetByIdAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _productService.GetByIdAsync(invalidId));
        }

        [Fact]
        public async Task GetByIdAsync_ProductNotFound_ReturnsNull()
        {
            // Arrange
            var productId = 1;
            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            var productId = 1;
            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _productService.GetByIdAsync(productId));
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ValidParameters_ReturnsPaginatedResult()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Category = "Electronics", Price = 100 },
                new Product { Id = 2, Name = "Product 2", Category = "Electronics", Price = 200 }
            };

            _mockProductRepository.Setup(x => x.GetAllAsync(10, 1, false))
                            .ReturnsAsync(products.AsQueryable());
            _mockProductRepository.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(2);

            // Act
            var result = await _productService.GetAllAsync(1, 10, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        public async Task GetAllAsync_InvalidPageNumber_ReturnsEmptyResult(int pageNumber, int pageSize)
        {
            // Act
            var result = await _productService.GetAllAsync(pageNumber, pageSize, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        public async Task GetAllAsync_InvalidPageSize_ReturnsEmptyResult(int pageNumber, int pageSize)
        {
            // Act
            var result = await _productService.GetAllAsync(pageNumber, pageSize, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetAllAsync_NoProductsFound_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetAllAsync(10, 1, false))
                     .ReturnsAsync(new List<Product>().AsQueryable());
            _mockProductRepository.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(0);

            // Act
            var result = await _productService.GetAllAsync(1, 10, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryThrowsException_ReturnsEmptyResult()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetAllAsync(10, 1, false))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _productService.GetAllAsync(1, 10, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        #endregion

        #region GetByCategoryAsync Tests

        [Fact]
        public async Task GetByCategoryAsync_ValidCategory_ReturnsPaginatedResult()
        {
            // Arrange
            var category = "Electronics";
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Category = category, Price = 100 }
            };

            _mockProductRepository.Setup(x => x.GetByCategoryAsync(category, 10, 1, false))
                    .ReturnsAsync(products.AsQueryable());
            _mockProductRepository.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _productService.GetByCategoryAsync(category, 1, 10, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetByCategoryAsync_InvalidCategory_ReturnsEmptyResult(string category)
        {
            // Act
            var result = await _productService.GetByCategoryAsync(category, 1, 10, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ValidProduct_ReturnsProductDTO()
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = "New Product",
                Category = "Electronics",
                Price = 100,
                MinimumQuantity = 5,
                DiscountRate = 10
            };

            var createdProduct = new Product
            {
                Id = 1,
                Name = createProductDto.Name,
                Category = createProductDto.Category,
                Price = createProductDto.Price,
                ProductCode = "P001"
            };

            _mockProductRepository.Setup(x => x.GenerateNextProductCodeAsync("P"))
                .ReturnsAsync("P001");
            _mockProductRepository.Setup(x => x.CreateAsync(It.IsAny<Product>()))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _productService.CreateAsync(createProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Product", result.Name);
            Assert.Equal("Electronics", result.Category);
        }

        [Fact]
        public async Task CreateAsync_NullProduct_ReturnsNull()
        {
            // Act
            var result = await _productService.CreateAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("", "Electronics", 100, 5, 10)]
        [InlineData("   ", "Electronics", 100, 5, 10)]
        [InlineData(null, "Electronics", 100, 5, 10)]
        public async Task CreateAsync_InvalidName_ReturnsNull(string name, string category, decimal price, int minQuantity, decimal discountRate)
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = name,
                Category = category,
                Price = price,
                MinimumQuantity = minQuantity,
                DiscountRate = discountRate
            };

            // Act
            var result = await _productService.CreateAsync(createProductDto);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("Product", "", 100, 5, 10)]
        [InlineData("Product", "   ", 100, 5, 10)]
        [InlineData("Product", null, 100, 5, 10)]
        public async Task CreateAsync_InvalidCategory_ReturnsNull(string name, string category, decimal price, int minQuantity, decimal discountRate)
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = name,
                Category = category,
                Price = price,
                MinimumQuantity = minQuantity,
                DiscountRate = discountRate
            };

            // Act
            var result = await _productService.CreateAsync(createProductDto);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateAsync_InvalidPrice_ReturnsNull(decimal price)
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = "Product",
                Category = "Electronics",
                Price = price,
                MinimumQuantity = 5,
                DiscountRate = 10
            };

            // Act
            var result = await _productService.CreateAsync(createProductDto);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateAsync_InvalidMinimumQuantity_ReturnsNull(int minQuantity)
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = "Product",
                Category = "Electronics",
                Price = 100,
                MinimumQuantity = minQuantity,
                DiscountRate = 10
            };

            // Act
            var result = await _productService.CreateAsync(createProductDto);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public async Task CreateAsync_InvalidDiscountRate_ReturnsNull(decimal discountRate)
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = "Product",
                Category = "Electronics",
                Price = 100,
                MinimumQuantity = 5,
                DiscountRate = discountRate
            };

            // Act
            var result = await _productService.CreateAsync(createProductDto);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidUpdate_ReturnsUpdatedProductDTO()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Product",
                Category = "Old Category",
                Price = 50
            };

            var updateProductDto = new UpdateProductDTO
            {
                Name = "Updated Product",
                Category = "Updated Category",
                Price = 100
            };

            var updatedProduct = new Product
            {
                Id = productId,
                Name = "Updated Product",
                Category = "Updated Category",
                Price = 100
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _productService.UpdateAsync(productId, updateProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Product", result.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UpdateAsync_InvalidId_ReturnsNull(int id)
        {
            // Arrange
            var updateProductDto = new UpdateProductDTO { Name = "Updated Product" };

            // Act
            var result = await _productService.UpdateAsync(id, updateProductDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_NullUpdateDto_ReturnsNull()
        {
            // Act
            var result = await _productService.UpdateAsync(1, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ProductNotFound_ReturnsNull()
        {
            // Arrange
            var productId = 1;
            var updateProductDto = new UpdateProductDTO { Name = "Updated Product" };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.UpdateAsync(productId, updateProductDto);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_SoftDelete_ReturnsTrue()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteAsync(productId, true);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(x => x.UpdateAsync(It.Is<Product>(p => p.IsDeleted == true)), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_HardDelete_ReturnsTrue()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockProductRepository.Setup(x => x.DeleteAsync(productId))
                .ReturnsAsync(true);

            // Act
            var result = await _productService.DeleteAsync(productId, false);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(x => x.DeleteAsync(productId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteAsync_InvalidId_ReturnsFalse(int id)
        {
            // Act
            var result = await _productService.DeleteAsync(id, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ProductNotFound_ReturnsFalse()
        {
            // Arrange
            var productId = 1;
            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.DeleteAsync(productId, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ProductWithImage_DeletesImageAndProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                ImagePath = "products/image.jpg"
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockFileService.Setup(x => x.DeleteFileAsync("products/image.jpg"))
                .ReturnsAsync(true); 
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteAsync(productId, true);

            // Assert
            Assert.True(result);
            _mockFileService.Verify(x => x.DeleteFileAsync("products/image.jpg"), Times.Once);
        }

        #endregion

        #region UploadImageAsync Tests

        [Fact]
        public async Task UploadImageAsync_ValidFile_ReturnsImagePath()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };
            var mockFile = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var expectedPath = "products/guid-generated-name.jpg";

            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockFileService.Setup(x => x.SaveFileAsync(mockFile.Object, "products"))
                .ReturnsAsync(expectedPath);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.UploadImageAsync(productId, mockFile.Object);

            // Assert
            Assert.Equal(expectedPath, result);
            _mockFileService.Verify(x => x.SaveFileAsync(mockFile.Object, "products"), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UploadImageAsync_InvalidId_ReturnsNull(int id)
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();

            // Act
            var result = await _productService.UploadImageAsync(id, mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadImageAsync_NullFile_ReturnsNull()
        {
            // Act
            var result = await _productService.UploadImageAsync(1, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadImageAsync_EmptyFile_ReturnsNull()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _productService.UploadImageAsync(1, mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("test.txt")]
        [InlineData("test.exe")]
        [InlineData("test")]
        public async Task UploadImageAsync_InvalidFileExtension_ReturnsNull(string fileName)
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns(fileName);

            // Act
            var result = await _productService.UploadImageAsync(1, mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadImageAsync_FileTooLarge_ReturnsNull()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            // Act
            var result = await _productService.UploadImageAsync(1, mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadImageAsync_ProductNotFound_ReturnsNull()
        {
            // Arrange
            var productId = 1;
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.UploadImageAsync(productId, mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadImageAsync_ProductWithExistingImage_DeletesOldImageFirst()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                ImagePath = "products/old-image.jpg"
            };
            var mockFile = new Mock<IFormFile>();
            var expectedPath = "products/new-image.jpg";

            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockFileService.Setup(x => x.DeleteFileAsync("products/old-image.jpg")).ReturnsAsync(true);
            _mockFileService.Setup(x => x.SaveFileAsync(mockFile.Object, "products"))
                .ReturnsAsync(expectedPath);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.UploadImageAsync(productId, mockFile.Object);

            // Assert
            Assert.Equal(expectedPath, result);
            _mockFileService.Verify(x => x.DeleteFileAsync("products/old-image.jpg"), Times.Once);
        }

        #endregion
    }
}