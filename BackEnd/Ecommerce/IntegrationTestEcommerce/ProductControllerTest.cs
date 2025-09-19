using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application.Services.ProductServices;
using DTOs.Product;
using DTOs.SharedData;
using Handmade.DTOs.SharedDTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Models;
using Moq;
using Newtonsoft.Json;
using System.Net;
namespace IntegrationTestEcommerce
{
    


        public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly HttpClient _client;
            private readonly Mock<IProductService> _mockProductService;

            public ProductsControllerTests(WebApplicationFactory<Program> factory)
            {
                _mockProductService = new Mock<IProductService>();

                var _factory = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll(typeof(IProductService));
                        services.AddSingleton(_mockProductService.Object);
                    });
                });

                _client = _factory.CreateClient();
            }

            [Fact]
            public async Task GetProducts_ReturnsOkResult()
            {
                var expectedProducts = new EntityPaginated<ProductDTO>
                {
                    Items = new List<ProductDTO>
                {
                    new ProductDTO { ProductId = 1, Name = "Product 1", Price = 10.99m },
                    new ProductDTO { ProductId = 2, Name = "Product 2", Price = 20.99m }
                },
                    TotalCount = 2,
                    PageNumber = 1,
                    PageSize = 10
                };

            _mockProductService.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedProducts);


            var response = await _client.GetAsync("/api/products");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<EntityPaginated<ProductDTO>>(content);
                Assert.Equal(2, result.Items.Count());
            }

            [Fact]
            public async Task GetProduct_ReturnsOkResult_WhenProductExists()
            {
                var productId = 1;
                var expectedProduct = new ProductDTO { ProductId = productId, Name = "Product 1", Price = 10.99m };

                _mockProductService.Setup(x => x.GetByIdAsync(productId))
                    .ReturnsAsync(expectedProduct);

                var response = await _client.GetAsync($"/api/products/{productId}");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductDTO>(content);
                Assert.Equal(productId, result.ProductId);
            }

            [Fact]
            public async Task GetProductsByCategory_ReturnsOkResult()
            {
                var category = "Electronics";
                var expectedProducts = new EntityPaginated<ProductDTO>
                {
                    Items = new List<ProductDTO>
                {
                    new ProductDTO { ProductId = 1, Name = "Laptop", Price = 1000m, Category = category },
                    new ProductDTO { ProductId = 2, Name = "Phone", Price = 500m, Category = category }
                },
                    TotalCount = 2,
                    PageNumber = 1,
                    PageSize = 10
                };

                _mockProductService.Setup(x => x.GetByCategoryAsync(category, 10, 1, false))
                    .ReturnsAsync(expectedProducts);

                var response = await _client.GetAsync($"/api/products/category/{category}?pageSize=10&pageNumber=1");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<EntityPaginated<ProductDTO>>(content);
                Assert.All(result.Items, p => Assert.Equal(category, p.Category));
            }

            [Fact]
            public async Task CreateProduct_ReturnsCreatedResult()
            {
                var newProduct = new CreateProductDTO { Name = "New Product", Price = 15.99m };
                var createdProduct = new ProductDTO { ProductId = 3, Name = "New Product", Price = 15.99m };

                _mockProductService.Setup(x => x.CreateAsync(newProduct))
                    .ReturnsAsync(createdProduct);

                var content = new StringContent(JsonConvert.SerializeObject(newProduct), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("/api/products", content);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductDTO>(responseContent);
                Assert.Equal(createdProduct.ProductId, result.ProductId);
            }


            [Fact]
            public async Task UpdateProduct_ReturnsOkResult_WhenProductExists()
            {
                var productId = 1;
                var updateProduct = new UpdateProductDTO { Name = "Updated Product", Price = 19.99m };
                var updatedProduct = new ProductDTO { ProductId = productId, Name = "Updated Product", Price = 19.99m };

                _mockProductService.Setup(x => x.UpdateAsync(productId, updateProduct))
                    .ReturnsAsync(updatedProduct);

                var content = new StringContent(JsonConvert.SerializeObject(updateProduct), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"/api/products/{productId}", content);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductDTO>(responseContent);
                Assert.Equal(updatedProduct.Name, result.Name);
            }

            [Fact]
            public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
            {
                var productId = 1;
                            _mockProductService
                .Setup(x => x.DeleteAsync(It.IsAny<int>(),It.IsAny<bool>()))
                .Returns(Task.FromResult(true));

            var response = await _client.DeleteAsync($"/api/products/{productId}");

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    

}
