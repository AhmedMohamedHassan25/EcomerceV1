using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.AuthService;
using DTOs.User;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Newtonsoft.Json;
using System.Net;
using DTOs.SharedData;

namespace IntegrationTestEcommerce
{
    

  
        public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly HttpClient _client;
            private readonly Mock<IAuthService> _mockAuthService;

            public AuthControllerTests(WebApplicationFactory<Program> factory)
            {
                _mockAuthService = new Mock<IAuthService>();

                var _factory = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll(typeof(IAuthService));
                        services.AddSingleton(_mockAuthService.Object);
                    });
                });

                _client = _factory.CreateClient();
            }

            [Fact]
        public async Task Register_ReturnsOk_WhenUserIsCreated()
        {
            var registerDto = new RegisterDTO
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var createdUser = new UserDTO
            {
                UserId = 1,
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            var authResponse = new AuthResponse
            {
                AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
                RefreshToken = "refresh-token-123456",
                Expires = DateTime.UtcNow.AddHours(1),
                User = new UserDTO
                {
                    UserId = 1,
                    UserName = "testuser",
                    Email = "test@example.com"
                }
            };

            _mockAuthService.Setup(x => x.RegisterAsync(registerDto))
                .ReturnsAsync(authResponse);

            var content = new StringContent(JsonConvert.SerializeObject(registerDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserDTO>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(createdUser.UserName, result.UserName);
        }

            [Fact]
        public async Task Login_ReturnsOk_WithToken()
        {
            var loginDto = new LoginDTO
            {
                UserName = "AhmedHassan",
                Password = "Password123!"
            };

            var authResponse = new AuthResponse
            {
                AccessToken = "fake-jwt-token",
                RefreshToken = "refresh-token-123456",
                Expires = DateTime.UtcNow.AddHours(1),
                User = new UserDTO
                {
                    UserId = 1,
                    UserName = "AhmedHassan",
                    Email = "ahmed@example.com"
                }
            };

            _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(authResponse);

            var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(authResponse.AccessToken, result.AccessToken);
        }
        }
    

}
