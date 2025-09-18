//using Application.Contract;
//using Application.Services.FileServices;
//using Application.Services.ProductServices;
//using DTOs.Product;
//using Mapster;
//using MapsterMapper;
//using Models;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EcommerceSiteTests
//{
//    public class ProductServiceTests
//    {
//        private readonly Mock<IProductRepository> _mockProductRepository;
//        private readonly Mock<IFileService> _mockFileService;
//        private readonly IMapper _mapper;

//        public ProductServiceTests()
//        {
//            _mockProductRepository = new Mock<IProductRepository>();
//            _mockFileService = new Mock<IFileService>();

//            ConfigureMappings();
//        }
//        private void ConfigureMappings()
//        {
           
//            TypeAdapterConfig<Product, ProductDTO>.NewConfig()
//                .Map(dest => dest.ProductId, src => src.Id)
//                .Map(dest => dest.ProductCode, src => src.ProductCode)
//                .Map(dest => dest.Name, src => src.Name)
//                .Map(dest => dest.Price, src => src.Price)
//                .Map(dest => dest.Category, src => src.Category)
//                .Map(dest => dest.MinimumQuantity, src => src.MinimumQuantity);

//            TypeAdapterConfig<CreateProductDTO, Product>.NewConfig()
//                .Map(dest => dest.Name, src => src.Name)
//                .Map(dest => dest.Price, src => src.Price)
//                .Map(dest => dest.Category, src => src.Category)
//                .Map(dest => dest.MinimumQuantity, src => src.MinimumQuantity)
//                .Ignore(dest => dest.Id)
//                .Ignore(dest => dest.ProductCode);
//        }
//        [Fact]
//        public async Task GetByIdAsync_ShouldReturnProductDto_WhenProductExists()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Category = "Electronics",
//                ProductCode = "P001",
//                Name = "Test Product",
//                Price = 99.99m,
//                MinimumQuantity = 5
//            };

//            _mockProductRepository.Setup(x => x.GetByIdAsync(1))
//                .ReturnsAsync(product);

//            var service = new ProductService(_mockProductRepository.Object, _mockFileService.Object);

//            // Act
//            var result = await service.GetByIdAsync(1);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(1, result.ProductId);
//            Assert.Equal("P001", result.ProductCode);
//        }

//        [Fact]
//        public async Task CreateAsync_ShouldReturnProductDto_WhenValidDataProvided()
//        {
//            // Arrange
//            var createDto = new CreateProductDTO
//            {
//                Category = "Electronics",
//                Name = "New Product",
//                Price = 199.99m,
//                MinimumQuantity = 10
//            };

//            _mockProductRepository.Setup(x => x.GenerateNextProductCodeAsync("P"))
//                .ReturnsAsync("P002");
//            _mockProductRepository.Setup(x => x.CreateAsync(It.IsAny<Product>()))
//                .ReturnsAsync((Product p) => { p.Id = 1; return p; });

//            var service = new ProductService(_mockProductRepository.Object, _mockFileService.Object);

//            // Act
//            var result = await service.CreateAsync(createDto);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("P002", result.ProductCode);
//            Assert.Equal("New Product", result.Name);
//            Assert.Equal(199.99m, result.Price);
//        }
//    }
//}
