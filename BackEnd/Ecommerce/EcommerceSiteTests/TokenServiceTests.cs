using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceSiteTests
{
    using global::Application.Services.TokenService;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Models;
    using Moq;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Xunit;

    namespace Application.Tests.Services
    {
        public class TokenServiceTests
        {
            private readonly Mock<IConfiguration> _mockConfiguration;
            private readonly Mock<ILogger<User>> _mockLogger;
            private readonly Mock<IConfigurationSection> _mockJwtSection;
            private readonly TokenService _tokenService;

            public TokenServiceTests()
            {
                _mockConfiguration = new Mock<IConfiguration>();
                _mockLogger = new Mock<ILogger<User>>();
                _mockJwtSection = new Mock<IConfigurationSection>();

                SetupConfiguration();
                _tokenService = new TokenService(_mockConfiguration.Object, _mockLogger.Object);
            }

            private void SetupConfiguration()
            {
                var jwtKey = "ThisIsAVerySecureKeyForJWTTokenGenerationWithAtLeast32Characters";
                var issuer = "TestIssuer";
                var audience = "TestAudience";

                _mockJwtSection.Setup(x => x["Key"]).Returns(jwtKey);
                _mockJwtSection.Setup(x => x["Issuer"]).Returns(issuer);
                _mockJwtSection.Setup(x => x["Audience"]).Returns(audience);

                _mockConfiguration.Setup(x => x.GetSection("JwtSettings"))
                    .Returns(_mockJwtSection.Object);
            }

            #region GenerateAccessToken Tests

            [Fact]
            public void GenerateAccessToken_ValidUser_ReturnsToken()
            {
                // Arrange
                var user = new User
                {
                    Id = 1,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                // Verify token structure
                var tokenHandler = new JwtSecurityTokenHandler();
                Assert.True(tokenHandler.CanReadToken(result));
            }

            [Fact]
            public void GenerateAccessToken_ValidUser_ContainsCorrectClaims()
            {
                // Arrange
                var user = new User
                {
                    Id = 1,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(result);

                Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
                Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
                Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
                Assert.Contains(token.Claims, c => c.Type == "jti");
            }

            [Fact]
            public void GenerateAccessToken_NullUser_ReturnsNull()
            {
                // Act
                var result = _tokenService.GenerateAccessToken(null);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GenerateAccessToken_UserWithInvalidId_ReturnsNull()
            {
                // Arrange
                var user = new User
                {
                    Id = 0,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                Assert.Null(result);
            }

            [Theory]
            [InlineData("")]
            [InlineData("   ")]
            [InlineData(null)]
            public void GenerateAccessToken_UserWithInvalidUserName_ReturnsNull(string userName)
            {
                // Arrange
                var user = new User
                {
                    Id = 1,
                    UserName = userName,
                    Email = "test@example.com"
                };

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                Assert.Null(result);
            }

            [Theory]
            [InlineData("")]
            [InlineData("   ")]
            [InlineData(null)]
            public void GenerateAccessToken_UserWithInvalidEmail_ReturnsNull(string email)
            {
                // Arrange
                var user = new User
                {
                    Id = 1,
                    UserName = "testuser",
                    Email = email
                };

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GenerateAccessToken_MissingJwtKey_ReturnsNull()
            {
                // Arrange
                _mockJwtSection.Setup(x => x["Key"]).Returns((string)null);
                var user = new User
                {
                    Id = 1,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GenerateAccessToken_TokenHasCorrectExpiration()
            {
                // Arrange
                var user = new User
                {
                    Id = 1,
                    UserName = "testuser",
                    Email = "test@example.com"
                };
                var beforeGeneration = DateTime.UtcNow;

                // Act
                var result = _tokenService.GenerateAccessToken(user);

                // Assert
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(result);
                var afterGeneration = DateTime.UtcNow;

                Assert.True(token.ValidTo > beforeGeneration.AddHours(1).AddMinutes(-1));
                Assert.True(token.ValidTo < afterGeneration.AddHours(1).AddMinutes(1));
            }

            #endregion

            #region GenerateRefreshToken Tests

            [Fact]
            public void GenerateRefreshToken_ReturnsValidBase64String()
            {
                // Act
                var result = _tokenService.GenerateRefreshToken();

                // Assert
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                // Verify it's a valid base64 string
                var bytes = Convert.FromBase64String(result);
                Assert.Equal(64, bytes.Length);
            }

            [Fact]
            public void GenerateRefreshToken_ReturnsUniqueTokens()
            {
                // Act
                var token1 = _tokenService.GenerateRefreshToken();
                var token2 = _tokenService.GenerateRefreshToken();

                // Assert
                Assert.NotEqual(token1, token2);
            }

            #endregion

            #region GetPrincipalFromExpiredToken Tests

            [Fact]
            public void GetPrincipalFromExpiredToken_ValidToken_ReturnsPrincipal()
            {
                // Arrange
                var user = new User
                {
                    Id = 1,
                    UserName = "testuser",
                    Email = "test@example.com"
                };

                var token = _tokenService.GenerateAccessToken(user);

                // Act
                var result = _tokenService.GetPrincipalFromExpiredToken(token);

                // Assert
                Assert.NotNull(result);
                Assert.Contains(result.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
                Assert.Contains(result.Claims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
                Assert.Contains(result.Claims, c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
            }

            [Theory]
            [InlineData("")]
            [InlineData("   ")]
            [InlineData(null)]
            public void GetPrincipalFromExpiredToken_InvalidToken_ReturnsNull(string token)
            {
                // Act
                var result = _tokenService.GetPrincipalFromExpiredToken(token);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GetPrincipalFromExpiredToken_MissingJwtKey_ReturnsNull()
            {
                // Arrange
                _mockJwtSection.Setup(x => x["Key"]).Returns((string)null);
                var token = "some.jwt.token";

                // Act
                var result = _tokenService.GetPrincipalFromExpiredToken(token);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GetPrincipalFromExpiredToken_InvalidTokenFormat_ReturnsNull()
            {
                // Arrange
                var invalidToken = "invalid.token.format";

                // Act
                var result = _tokenService.GetPrincipalFromExpiredToken(invalidToken);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GetPrincipalFromExpiredToken_TokenWithWrongAlgorithm_ReturnsNull()
            {
                // Arrange
                var key = Encoding.ASCII.GetBytes("ThisIsAVerySecureKeyForJWTTokenGenerationWithAtLeast32Characters");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "testuser"),
                    new Claim(ClaimTypes.Email, "test@example.com")
                }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha512) // Different algorithm
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Act
                var result = _tokenService.GetPrincipalFromExpiredToken(tokenString);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void GetPrincipalFromExpiredToken_ExpiredToken_StillReturnsPrincipal()
            {
                // Arrange
                var key = Encoding.ASCII.GetBytes("ThisIsAVerySecureKeyForJWTTokenGenerationWithAtLeast32Characters");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "testuser"),
                    new Claim(ClaimTypes.Email, "test@example.com")
                }),
                    Expires = DateTime.UtcNow.AddHours(-1), 
                    Issuer = "TestIssuer",
                    Audience = "TestAudience",
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Act
                var result = _tokenService.GetPrincipalFromExpiredToken(tokenString);

                // Assert
                Assert.NotNull(result);
                Assert.Contains(result.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
            }

            #endregion
        }
    }
}
