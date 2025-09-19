using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Contract;
using Application.Services.UserServices;
using DTOs.User;
using global::Application.Contract;
using global::Application.Services.UserServices;
using Handmade.DTOs.SharedDTOs;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EcommerceSiteTests
{


        public class UserServiceTests
        {
            private readonly Mock<IUserRepository> _mockUserRepository;
            private readonly Mock<ILogger<User>> _mockLogger;
            private readonly UserService _userService;

            public UserServiceTests()
            {
                _mockUserRepository = new Mock<IUserRepository>();
                _mockLogger = new Mock<ILogger<User>>();
                _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
            }

            #region GetByIdAsync Tests

            [Fact]
            public async Task GetByIdAsync_ValidId_ReturnsUserDTO()
            {
                // Arrange
                var userId = 1;
                var user = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "test@example.com",
                    Password = "hashedpassword"
                };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(user);

                // Act
                var result = await _userService.GetByIdAsync(userId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(userId, result.UserId);
                Assert.Equal("testuser", result.UserName);
                Assert.Equal("test@example.com", result.Email);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task GetByIdAsync_InvalidId_ReturnsNull(int invalidId)
            {
                // Act
                var result = await _userService.GetByIdAsync(invalidId);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task GetByIdAsync_UserNotFound_ReturnsNull()
            {
                // Arrange
                var userId = 1;
                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync((User)null);

                // Act
                var result = await _userService.GetByIdAsync(userId);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task GetByIdAsync_RepositoryThrowsException_ReturnsNull()
            {
                // Arrange
                var userId = 1;
                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _userService.GetByIdAsync(userId);

                // Assert
                Assert.Null(result);
            }

            #endregion

            #region GetAllAsync Tests

            [Fact]
        public async Task GetAllAsync_ValidParameters_ReturnsPaginatedResult()
        {
            // Arrange
            var users = new List<User>
           {
               new User { Id = 1, UserName = "user1", Email = "user1@example.com" },
               new User { Id = 2, UserName = "user2", Email = "user2@example.com" }
           };

            _mockUserRepository.Setup(x => x.GetAllAsync(10, 1, false))
                .ReturnsAsync(users.AsQueryable());  
            _mockUserRepository.Setup(x => x.CountAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(2);

            // Act
            var result = await _userService.GetAllAsync(1, 10, false);

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
                var result = await _userService.GetAllAsync(pageNumber, pageSize, false);

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
                var result = await _userService.GetAllAsync(pageNumber, pageSize, false);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Items);
                Assert.Equal(0, result.TotalCount);
            }

            [Fact]
            public async Task GetAllAsync_NoUsersFound_ReturnsEmptyPaginatedResult()
            {
                // Arrange
                _mockUserRepository.Setup(x => x.GetAllAsync(10, 1, false))
                    .ReturnsAsync(new List<User>().AsQueryable());
                _mockUserRepository.Setup(x => x.CountAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .ReturnsAsync(0);

                // Act
                var result = await _userService.GetAllAsync(1, 10, false);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Items);
                Assert.Equal(0, result.TotalCount);
            }

            [Fact]
            public async Task GetAllAsync_RepositoryThrowsException_ReturnsEmptyResult()
            {
                // Arrange
                _mockUserRepository.Setup(x => x.GetAllAsync(10, 1, false))
                    .ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _userService.GetAllAsync(1, 10, false);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Items);
                Assert.Equal(0, result.TotalCount);
            }

            #endregion

            #region UpdateAsync Tests

            [Fact]
            public async Task UpdateAsync_ValidUpdate_ReturnsUpdatedUserDTO()
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "olduser",
                    Email = "old@example.com",
                    Password = "oldhashedpassword"
                };

                var updateUserDto = new UpdateUserDTO
                {
                    Email = "new@example.com",
                    Password = "newpassword123"
                };

                var updatedUser = new User
                {
                    Id = userId,
                    UserName = "olduser",
                    Email = "new@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("newpassword123")
                };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);
                _mockUserRepository.Setup(x => x.EmailExistsAsync("new@example.com"))
                    .ReturnsAsync(false);
                _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .ReturnsAsync(updatedUser);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("new@example.com", result.Email);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task UpdateAsync_InvalidId_ReturnsNull(int id)
            {
                // Arrange
                var updateUserDto = new UpdateUserDTO { Email = "new@example.com" };

                // Act
                var result = await _userService.UpdateAsync(id, updateUserDto);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task UpdateAsync_NullUpdateDto_ReturnsNull()
            {
                // Act
                var result = await _userService.UpdateAsync(1, null);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task UpdateAsync_UserNotFound_ReturnsNull()
            {
                // Arrange
                var userId = 1;
                var updateUserDto = new UpdateUserDTO { Email = "new@example.com" };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync((User)null);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.Null(result);
            }

            [Theory]
            [InlineData("")]
            [InlineData("   ")]
            public async Task UpdateAsync_EmptyEmail_ReturnsNull(string email)
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "old@example.com"
                };

                var updateUserDto = new UpdateUserDTO { Email = email };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task UpdateAsync_EmailAlreadyExists_ReturnsNull()
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "old@example.com"
                };

                var updateUserDto = new UpdateUserDTO { Email = "existing@example.com" };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);
                _mockUserRepository.Setup(x => x.EmailExistsAsync("existing@example.com"))
                    .ReturnsAsync(true);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task UpdateAsync_SameEmailAsCurrentUser_UpdatesSuccessfully()
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "current@example.com"
                };

                var updateUserDto = new UpdateUserDTO { Email = "current@example.com" };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);
                _mockUserRepository.Setup(x => x.EmailExistsAsync("current@example.com"))
                    .ReturnsAsync(true);
                _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .ReturnsAsync(existingUser);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("current@example.com", result.Email);
            }

            [Theory]
            [InlineData("")]
            [InlineData("   ")]
            public async Task UpdateAsync_EmptyPassword_ReturnsNull(string password)
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                var updateUserDto = new UpdateUserDTO { Password = password };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public async Task UpdateAsync_PasswordTooShort_ReturnsNull()
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                var updateUserDto = new UpdateUserDTO { Password = "short" }; // Less than 8 characters

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.Null(result);
            }

           

        [Fact]
            public async Task UpdateAsync_EmailNormalization_TrimsAndLowercases()
            {
                // Arrange
                var userId = 1;
                var existingUser = new User
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = "old@example.com"
                };

                var updateUserDto = new UpdateUserDTO { Email = "  NEW@EXAMPLE.COM  " };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);
                _mockUserRepository.Setup(x => x.EmailExistsAsync("new@example.com"))
                    .ReturnsAsync(false);
                _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .ReturnsAsync((User user) => user);

                // Act
                var result = await _userService.UpdateAsync(userId, updateUserDto);

                // Assert
                Assert.NotNull(result);
                _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                    u.Email == "new@example.com")), Times.Once);
            }

            #endregion

            #region DeleteAsync Tests

            [Fact]
            public async Task DeleteAsync_SoftDelete_ReturnsTrue()
            {
                // Arrange
                var userId = 1;
                var user = new User { Id = userId, UserName = "testuser" };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(user);
                _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .ReturnsAsync(user);

                // Act
                var result = await _userService.DeleteAsync(userId, true);

                // Assert
                Assert.True(result);
                _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => u.IsDeleted == true)), Times.Once);
            }

            [Fact]
            public async Task DeleteAsync_HardDelete_ReturnsTrue()
            {
                // Arrange
                var userId = 1;
                var user = new User { Id = userId, UserName = "testuser" };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(user);
                _mockUserRepository.Setup(x => x.DeleteAsync(userId))
                    .ReturnsAsync(true);

                // Act
                var result = await _userService.DeleteAsync(userId, false);

                // Assert
                Assert.True(result);
                _mockUserRepository.Verify(x => x.DeleteAsync(userId), Times.Once);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task DeleteAsync_InvalidId_ReturnsFalse(int id)
            {
                // Act
                var result = await _userService.DeleteAsync(id, true);

                // Assert
                Assert.False(result);
            }

            [Fact]
            public async Task DeleteAsync_UserNotFound_ReturnsFalse()
            {
                // Arrange
                var userId = 1;
                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync((User)null);

                // Act
                var result = await _userService.DeleteAsync(userId, true);

                // Assert
                Assert.False(result);
            }

            [Fact]
            public async Task DeleteAsync_RepositoryThrowsException_ReturnsFalse()
            {
                // Arrange
                var userId = 1;
                var user = new User { Id = userId, UserName = "testuser" };

                _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                    .ReturnsAsync(user);
                _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                    .ThrowsAsync(new Exception("Database error"));

                // Act
                var result = await _userService.DeleteAsync(userId, true);

                // Assert
                Assert.False(result);
            }

            #endregion
        }
    
}
