using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract
{
    public interface  IUserRepository
    {
        public Task<User> GetByIdAsync(int id);
        public Task<User> GetByUserNameAsync(string userName);
        public Task<User> GetByEmailAsync(string email);
        public Task<User> GetByRefreshTokenAsync(string refreshToken);
        public Task<IQueryable<User>> GetAllAsync(int Pagesize = 10, int pageNumber = 1, bool IsDeleted = false);
        public Task<User> CreateAsync(User user);
        public Task<User> UpdateAsync(User user);
        public Task<bool> DeleteAsync(int id);
        public Task<bool> UserExistsAsync(int id);
        public Task<bool> UserNameExistsAsync(string userName);
        public Task<bool> EmailExistsAsync(string email);
        public Task<int> CountAsync();
        public Task<int> CountAsync(Expression<Func<User, bool>> predicate);
    }
}
