using Application.Contract;
using DTOs.Product;
using DTOs.User;
using Handmade.DTOs.SharedDTOs;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.UserServices
{
    public class UserService(IUserRepository _userRepository,ILogger<User> _logger) : IUserService
    {


        public async Task<UserDTO> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting user with ID: {UserId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided: {UserId}, returning null", id);
                    return null;
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found, returning null", id);
                    return null;
                }

                var userDto = user.Adapt<UserDTO>();
                _logger.LogInformation("Successfully retrieved user with ID: {UserId}", id);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user with ID: {UserId}, returning null", id);
                return null;
            }
        }

        public async Task<EntityPaginated<UserDTO>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool isDeleted = false)
        {
            try
            {
                _logger.LogInformation("Getting users - Page: {PageNumber}, Size: {PageSize}, IsDeleted: {IsDeleted}",
                    pageNumber, pageSize, isDeleted);

                if (pageNumber < 1)
                {
                    _logger.LogWarning("Invalid page number provided: {PageNumber}, returning empty result", pageNumber);
                    return new EntityPaginated<UserDTO>
                    {
                        Items = Enumerable.Empty<UserDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                if (pageSize < 1)
                {
                    _logger.LogWarning("Invalid page size provided: {PageSize}, returning empty result", pageSize);
                    return new EntityPaginated<UserDTO>
                    {
                        Items = Enumerable.Empty<UserDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                var getUsersTask = _userRepository.GetAllAsync(pageSize, pageNumber, isDeleted);
                var getTotalCountTask = _userRepository.CountAsync(u => u.IsDeleted == isDeleted);

                await Task.WhenAll(getUsersTask, getTotalCountTask);

                var users = await getUsersTask;
                var totalCount = await getTotalCountTask;

                _logger.LogInformation("Retrieved {UserCount} users out of {TotalCount} total users",
                    users.Count(), totalCount);

                if (!users.Any())
                {
                    _logger.LogInformation("No users found for the specified criteria");
                    return new EntityPaginated<UserDTO>
                    {
                        Items = Enumerable.Empty<UserDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    };
                }

                var userDtos = users.Adapt<IEnumerable<UserDTO>>();
                var entityPaginated = new EntityPaginated<UserDTO>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                _logger.LogInformation("Successfully returned paginated users - Page {PageNumber}, " +
                    "Items: {ItemCount}",
                    pageNumber, userDtos.Count());

                return entityPaginated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users - Page: {PageNumber}, " +
                    "Size: {PageSize}, IsDeleted: {IsDeleted}, returning empty result", pageNumber, pageSize, isDeleted);

                return new EntityPaginated<UserDTO>
                {
                    Items = Enumerable.Empty<UserDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        public async Task<UserDTO> UpdateAsync(int id, UpdateUserDTO updateUserDto)
        {
            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided: {UserId}, returning null", id);
                    return null;
                }

                if (updateUserDto == null)
                {
                    _logger.LogWarning("UpdateUserDTO is null for user ID: {UserId}, returning null", id);
                    return null;
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found for update, returning null", id);
                    return null;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                {
                    if (string.IsNullOrWhiteSpace(updateUserDto.Email))
                    {
                        _logger.LogWarning("Empty email provided for user ID: {UserId}, returning null", id);
                        return null;
                    }

                    if (await _userRepository.EmailExistsAsync(updateUserDto.Email) && user.Email != updateUserDto.Email)
                    {
                        _logger.LogWarning("Email already exists: {Email} for user ID: {UserId}, returning null",
                            updateUserDto.Email, id);
                        return null;
                    }

                    user.Email = updateUserDto.Email.Trim().ToLowerInvariant();
                }

                if (!string.IsNullOrEmpty(updateUserDto.Password))
                {
                    if (string.IsNullOrWhiteSpace(updateUserDto.Password))
                    {
                        _logger.LogWarning("Empty password provided for user ID: {UserId}, returning null", id);
                        return null;
                    }

                    if (updateUserDto.Password.Length < 8)
                    {
                        _logger.LogWarning("Password too short for user ID: {UserId}, returning null", id);
                        return null;
                    }

                    user.Password = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
                    _logger.LogInformation("Password updated for user ID: {UserId}", id);
                }

                user = await _userRepository.UpdateAsync(user);
                var userDto = user.Adapt<UserDTO>();

                _logger.LogInformation("Successfully updated user with ID: {UserId}", id);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID: {UserId}, returning null", id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id, bool soft)
        {
            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}, Soft delete: {SoftDelete}", id, soft);

                // Validation
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided for deletion: {UserId}, returning false", id);
                    return false;
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found for deletion, returning false", id);
                    return false;
                }

                bool result;
                if (soft)
                {
                    user.IsDeleted = true;
                    await _userRepository.UpdateAsync(user);
                    result = true;
                    _logger.LogInformation("Successfully soft deleted user with ID: {UserId}", id);
                }
                else
                {
                    result = await _userRepository.DeleteAsync(id);
                    _logger.LogInformation("Successfully hard deleted user with ID: {UserId}, Result: {Result}",
                        id, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {UserId}, returning false", id);
                return false;
            }
        }

        
    }
}
