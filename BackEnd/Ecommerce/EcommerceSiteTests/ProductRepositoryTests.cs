using Context;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceSiteTests
{

    public class ProductRepositoryTests
    {
        private ECommerceContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ECommerceContext(options);
        }

        [Fact]
        public async Task GetByProductCodeAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ProductRepositroy(context);
            var product = new Product
            {
                Category = "Electronics",
                ProductCode = "P001",
                Name = "Test Product",
                Price = 99.99m,
                MinimumQuantity = 5
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByProductCodeAsync("P001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProductCode);
        }

        [Fact]
        public async Task GenerateNextProductCodeAsync_ShouldReturnP001_WhenNoProductsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ProductRepositroy(context);

            // Act
            var result = await repository.GenerateNextProductCodeAsync("P");

            // Assert
            Assert.Equal("P001", result);
        }

        [Fact]
        public async Task GenerateNextProductCodeAsync_ShouldReturnNextCode_WhenProductsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ProductRepositroy(context);

            var product = new Product
            {
                Category = "Electronics",
                ProductCode = "P005",
                Name = "Test Product",
                Price = 99.99m,
                MinimumQuantity = 5
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GenerateNextProductCodeAsync("P");

            // Assert
            Assert.Equal("P006", result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddProduct_WhenValidProductProvided()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ProductRepositroy(context);
            var product = new Product
            {
                Category = "Electronics",
                ProductCode = "P001",
                Name = "New Product",
                Price = 199.99m,
                MinimumQuantity = 10
            };

            // Act
            var result = await repository.CreateAsync(product);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("P001", result.ProductCode);

            var productInDb = await context.Products.FindAsync(result.Id);
            Assert.NotNull(productInDb);
        }
    }

}
