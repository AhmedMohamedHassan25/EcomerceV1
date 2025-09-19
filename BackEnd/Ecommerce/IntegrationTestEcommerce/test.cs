
using System.Text;
using Application.Services.UserServices;
using DTOs.User;
using Handmade.DTOs.SharedDTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
namespace IntegrationTestEcommerce
{
   

    namespace ClientWebSiteApi.IntegrationTests
    {
        public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly WebApplicationFactory<Program> _factory;
            private readonly HttpClient _client;
            private readonly Mock<IUserService> _mockUserService;

            public UsersControllerTests(WebApplicationFactory<Program> factory)
            {
                _mockUserService = new Mock<IUserService>();

                _factory = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll(typeof(IUserService));
                        services.AddSingleton(_mockUserService.Object);
                    });
                });

                _client = _factory.CreateClient();
            }

            [Fact]
            public async Task GetUsers_ReturnsOkResult()
            {
                // Arrange
                var expectedUsers = new EntityPaginated<UserDTO>
                {
                    Items = new List<UserDTO>
                {
                    new UserDTO { UserId = 1, Email = "user1@example.com" },
                    new UserDTO { UserId = 2, Email = "user2@example.com" }
                },
                    TotalCount = 2,
                    PageNumber = 1,
                    PageSize = 10
                };

                _mockUserService
                         .Setup(x => x.GetAllAsync(It.IsAny<int>(),It.IsAny<int>(),It.IsAny<bool>()))
                         .ReturnsAsync(expectedUsers);
                SetupAuthenticatedUser();

                // Act
                var response = await _client.GetAsync("/api/users");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            [Fact]
            public async Task GetUser_WithValidId_ReturnsOkResult()
            {
                // Arrange
                var userId = 1;
                var expectedUser = new UserDTO { UserId = userId, Email = "test@example.com" };

                _mockUserService.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(expectedUser);

                SetupAuthenticatedUser();

                // Act
                var response = await _client.GetAsync($"/api/users/{userId}");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            [Fact]
            public async Task GetUser_WithInvalidId_ReturnsNotFound()
            {
                // Arrange
                var userId = 999;
                _mockUserService.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync((UserDTO)null);

                SetupAuthenticatedUser();

                // Act
                var response = await _client.GetAsync($"/api/users/{userId}");

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            [Fact]
            public async Task UpdateUser_WithValidData_ReturnsOkResult()
            {
                // Arrange
                var userId = 1;
                var updateUserDto = new UpdateUserDTO
                {
                    Email = "updated@example.com",
                    Password = "222333331111"
                };

                var updatedUser = new UserDTO
                {
                    UserId = userId,
                    Email = updateUserDto.Email,
                };

                _mockUserService.Setup(x => x.UpdateAsync(userId, It.IsAny<UpdateUserDTO>()))
                    .ReturnsAsync(updatedUser);

                SetupAuthenticatedUser(userId);

                var json = JsonConvert.SerializeObject(updateUserDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PutAsync($"/api/users/{userId}", content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            [Fact]
            public async Task UpdateUser_WithDifferentUserId_ReturnsForbidden()
            {
                // Arrange
                var userId = 1;
                var differentUserId = 2;
                var updateUserDto = new UpdateUserDTO { Email = "updated@example.com" };

                SetupAuthenticatedUser(differentUserId);

                var json = JsonConvert.SerializeObject(updateUserDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PutAsync($"/api/users/{userId}", content);

                // Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }

            [Fact]
            public async Task DeleteUser_WithValidId_ReturnsOkResult()
            {
                // Arrange
                var userId = 1;
                _mockUserService.Setup(x => x.DeleteAsync(userId, true))
                    .ReturnsAsync(true);

                SetupAuthenticatedUser(userId);

                // Act
                var response = await _client.DeleteAsync($"/api/users/{userId}");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            [Fact]
            public async Task GetProfile_ReturnsOkResult()
            {
                // Arrange
                var userId = 1;
                var expectedUser = new UserDTO { UserId = userId, Email = "test@example.com" };

                _mockUserService.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(expectedUser);

                SetupAuthenticatedUser(userId);

                // Act
                var response = await _client.GetAsync("/api/users/profile");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            private void SetupAuthenticatedUser(int userId = 1)
            {
                var token = $"test-jwt-token-for-user-{userId}";
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

}
