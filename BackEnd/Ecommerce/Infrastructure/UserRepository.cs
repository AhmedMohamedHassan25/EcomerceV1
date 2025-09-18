using Application.Contract;
using Context;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Expressions;


namespace Infrastructure
{
    public class UserRepository:IUserRepository
    {
      
            private readonly ECommerceContext _context;

            public UserRepository(ECommerceContext context)
            {
                _context = context;
            }

            public async Task<User> GetByIdAsync(int id)=> 
                    await _context.Users.FindAsync(id);


            public async Task<User> GetByUserNameAsync(string userName)=> 
                    await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        

            public async Task<User> GetByEmailAsync(string email)=> 
                    await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            

            public async Task<User> GetByRefreshTokenAsync(string refreshToken)=>
                   await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);


           public Task<IQueryable<User>> GetAllAsync(int Pagesize = 0, int pageNumber = 1, bool IsDeleted = false) =>
                       Task.FromResult(_context.Users
                            .Where(b => b.IsDeleted == IsDeleted)
                            .Skip((pageNumber - 1) * Pagesize)
                            .Take(Pagesize)
                            .AsQueryable());


            public async Task<User> CreateAsync(User user)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }

            public async Task<User> UpdateAsync(User user)
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return user;
            }


            public async Task<bool> DeleteAsync(int id)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task<bool> UserExistsAsync(int id)=>
                     await _context.Users.AnyAsync(u => u.Id == id);
         

            public async Task<bool> UserNameExistsAsync(string userName)=> 
                await _context.Users.AnyAsync(u => u.UserName == userName);
            

            public async Task<bool> EmailExistsAsync(string email)=>
                    await _context.Users.AnyAsync(u => u.Email == email);

            public virtual async Task<int> CountAsync() => await _context.Users.CountAsync();

            public virtual async Task<int> CountAsync(Expression<Func<User, bool>> predicate) => 
                    await _context.Users.CountAsync(predicate);

    }
}
