using Application.Services.AuthService;
using DTOs.SharedData;
using DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClientWebSiteApi.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly IAuthService _authService;

            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            [HttpPost("login")]
            public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginDTO loginDto)
            {
                try
                {
                    var result = await _authService.LoginAsync(loginDto);
                    return Ok(Result<AuthResponse>.Success(result));
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(Result<AuthResponse>.Failure(ex.Message));
                }
            }

            [HttpPost("register")]
            public async Task<ActionResult<Result<AuthResponse>>> Register([FromBody] RegisterDTO registerDto)
            {
                try
                {
                    var result = await _authService.RegisterAsync(registerDto);
                    return Ok(Result<AuthResponse>.Success(result));
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(Result<AuthResponse>.Failure(ex.Message));
                }
            }

            [HttpPost("refresh")]
            public async Task<ActionResult<Result<AuthResponse>>> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
            {
                try
                {
                    var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                    return Ok(Result<AuthResponse>.Success(result));
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(Result<AuthResponse>.Failure(ex.Message));
                }
            }

            [HttpPost("revoke")]
            [Authorize]
            public async Task<ActionResult<Result>> Revoke([FromBody] RefreshTokenDto refreshTokenDto)
            {
                var result = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);

                if (result)
                    return Ok(Result.Success());

                return BadRequest(Result.Failure("Failed to revoke token"));
            }
        }
 }
