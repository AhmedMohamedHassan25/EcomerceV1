
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.TokenService
{

    public class TokenService(IConfiguration _configuration,ILogger<User> _logger) : ITokenService
    {

        public string GenerateAccessToken(User user)
        {
            try
            {
                _logger.LogInformation("Generating access token for user ID: {UserId}", user?.Id);

                if (user == null)
                {
                    _logger.LogWarning("User is null, cannot generate access token, returning null");
                    return null;
                }

                if (user.Id <= 0)
                {
                    _logger.LogWarning("Invalid user ID: {UserId}, returning null", user.Id);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(user.UserName))
                {
                    _logger.LogWarning("User name is null or empty for user ID: {UserId}, returning null", user.Id);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    _logger.LogWarning("User email is null or empty for user ID: {UserId}, returning null", user.Id);
                    return null;
                }

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var keyString = jwtSettings["Key"];

                if (string.IsNullOrWhiteSpace(keyString))
                {
                    _logger.LogError("JWT Key is not configured, returning null");
                    return null;
                }

                var key = Encoding.ASCII.GetBytes(keyString);
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("jti", Guid.NewGuid().ToString())
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Successfully generated access token for user ID: {UserId}", user.Id);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating access token for user ID: {UserId}, returning null", user?.Id);
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            try
            {
                _logger.LogInformation("Generating refresh token");

                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var refreshToken = Convert.ToBase64String(randomBytes);

                _logger.LogInformation("Successfully generated refresh token");
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating refresh token, returning null");
                return null;
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                _logger.LogInformation("Getting principal from expired token");

              
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Token is null or empty, returning null");
                    return null;
                }

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var keyString = jwtSettings["Key"];

                if (string.IsNullOrWhiteSpace(keyString))
                {
                    _logger.LogError("JWT Key is not configured, returning null");
                    return null;
                }

                var key = Encoding.ASCII.GetBytes(keyString);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = false,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"]
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm or format, returning null");
                    return null;
                }

                _logger.LogInformation("Successfully retrieved principal from expired token");
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Security token exception occurred, returning null");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting principal from expired token, returning null");
                return null;
            }
        }
    }

}
