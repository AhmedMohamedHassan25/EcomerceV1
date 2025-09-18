using Application.Contract;
using Application.Mapper;
using Application.Services.AuthService;
using Application.Services.TokenService;
using DTOs.User;
using Mapster;
using MapsterMapper;
using Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceSiteTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITokenService> _mockTokenService;
        TypeAdapterConfig config;
        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();

            config = new TypeAdapterConfig();
            config.NewConfig<User, UserDTO>();
            config.NewConfig<RegisterDTO, User>();
            config.NewConfig<LoginDTO, User>();

           
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnAuthResponse_WhenValidCredentialsProvided()
        {
            // Arrange
            var loginDto = new LoginDTO { UserName = "testuser", Password = "password123" };
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            };

            _mockUserRepository.Setup(x => x.GetByUserNameAsync("testuser"))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(user);
            _mockTokenService.Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var service = new AuthService(_mockUserRepository.Object, _mockTokenService.Object);

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("testuser", result.User.UserName);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenInvalidCredentialsProvided()
        {
            // Arrange
            var loginDto = new LoginDTO { UserName = "testuser", Password = "wrongpassword" };
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            _mockUserRepository.Setup(x => x.GetByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            var service = new AuthService(_mockUserRepository.Object, _mockTokenService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnAuthResponse_WhenValidDataProvided()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(x => x.UserNameExistsAsync("newuser"))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.EmailExistsAsync("newuser@example.com"))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.Id = 1; return u; });
            _mockTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("access-token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var service = new AuthService(_mockUserRepository.Object, _mockTokenService.Object);

            // Act
            var result = await service.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("newuser", result.User.UserName);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenUserNameExists()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                UserName = "existinguser",
                Email = "new@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(x => x.UserNameExistsAsync("existinguser"))
                .ReturnsAsync(true);

            var service = new AuthService(_mockUserRepository.Object, _mockTokenService.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(registerDto));
            Assert.Equal("Username already exists", exception.Message);
        }
    }
}
