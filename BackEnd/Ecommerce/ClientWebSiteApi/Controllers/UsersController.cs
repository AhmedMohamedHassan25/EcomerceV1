using Application.Services.UserServices;
using DTOs.SharedData;
using DTOs.User;
using Handmade.DTOs.SharedDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientWebSiteApi.Controllers
{
 
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class UsersController : ControllerBase
        {
            private readonly IUserService _userService;

            public UsersController(IUserService userService)
            {
                _userService = userService;
            }

            [HttpGet]
            public async Task<ActionResult<Result<EntityPaginated<UserDTO>>>> GetUsers()
            {
                var users = await _userService.GetAllAsync();
                return Ok(Result<EntityPaginated<UserDTO>>.Success(users));
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Result<UserDTO>>> GetUser(int id)
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(Result<UserDTO>.Failure("User not found"));
                }
                return Ok(Result<UserDTO>.Success(user));
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<Result<UserDTO>>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
            {
                try
                {
                    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    if (currentUserId != id)
                    {
                        return Forbid();
                    }

                    var user = await _userService.UpdateAsync(id, updateUserDto);
                    if (user == null)
                    {
                        return NotFound(Result<UserDTO>.Failure("User not found"));
                    }
                    return Ok(Result<UserDTO>.Success(user));
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(Result<UserDTO>.Failure(ex.Message));
                }
            }

            [HttpDelete("{id}")]
            public async Task<ActionResult<Result>> DeleteUser(int id)
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId != id)
                {
                    return Forbid();
                }

                var deleted = await _userService.DeleteAsync(id,true);
                if (!deleted)
                {
                    return NotFound(Result.Failure("User not found"));
                }
                return Ok(Result.Success());
            }

            [HttpGet("profile")]
            public async Task<ActionResult<Result<UserDTO>>> GetProfile()
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(Result<UserDTO>.Failure("User not found"));
                }
                return Ok(Result<UserDTO>.Success(user));
            }
        }
    

}
