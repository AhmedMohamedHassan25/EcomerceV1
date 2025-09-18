using Application.Contract;
using Application.Services.TokenService;
using DTOs.SharedData;
using DTOs.User;
using Mapster;
using MapsterMapper;
using Models;


namespace Application.Services.AuthService
{
    public class AuthService(IUserRepository _userRepository, ITokenService _tokenService) : IAuthService
    {
  

        public async Task<AuthResponse> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            user.LastLoginTime = DateTime.UtcNow;
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);

            var userDto =  user.Adapt<UserDTO>();
            var accessToken = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                Expires = DateTime.UtcNow.AddHours(1),
                User = userDto
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDTO registerDto)
        {
            if (await _userRepository.UserNameExistsAsync(registerDto.UserName))
                throw new ArgumentException("Username already exists");

            if (await _userRepository.EmailExistsAsync(registerDto.Email))
                throw new ArgumentException("Email already exists");

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };

            user = await _userRepository.CreateAsync(user);
            var userDto = user.Adapt<UserDTO>();
            var accessToken = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                Expires = DateTime.UtcNow.AddHours(1),
                User = userDto
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            var userDto = user.Adapt<UserDTO>();
            var accessToken = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken,
                Expires = DateTime.UtcNow.AddHours(1),
                User = userDto
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}
