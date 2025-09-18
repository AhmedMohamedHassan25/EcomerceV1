using DTOs.Product;
using DTOs.User;
using Handmade.DTOs.SharedDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.UserServices
{
    public interface IUserService
    {
        public Task<UserDTO> GetByIdAsync(int id);
        public Task<EntityPaginated<UserDTO>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool IsDeleted = false);
        public Task<UserDTO> UpdateAsync(int id, UpdateUserDTO updateUserDto);
        public Task<bool> DeleteAsync(int id,bool soft);
    }
}
