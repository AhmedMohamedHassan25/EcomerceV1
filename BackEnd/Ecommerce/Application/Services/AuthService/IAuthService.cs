using DTOs.SharedData;
using DTOs.User;

namespace Application.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginDTO loginDto);
        Task<AuthResponse> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}
