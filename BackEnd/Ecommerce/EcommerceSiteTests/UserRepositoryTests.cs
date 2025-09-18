using Context;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;

namespace EcommerceSiteTests
{

    public class UserRepositoryTests
    {
        private ECommerceContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ECommerceContext(options);
        }

        [Fact]
        public async Task GetByUserNameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserNameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.UserName);
        }

        [Fact]
        public async Task GetByUserNameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetByUserNameAsync("nonexistentuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUser_WhenValidUserProvided()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            var user = new User
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "hashedpassword"
            };

            // Act
            var result = await repository.CreateAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("newuser", result.UserName);

            var userInDb = await context.Users.FindAsync(result.Id);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task UserNameExistsAsync_ShouldReturnTrue_WhenUserNameExists()
        {
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            var user = new User
            {
                UserName = "existinguser",
                Email = "existing@example.com",
                Password = "hashedpassword"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await repository.UserNameExistsAsync("existinguser");

            Assert.True(result);
        }
    }


}