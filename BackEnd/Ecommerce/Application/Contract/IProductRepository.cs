using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract
{
    public interface IProductRepository
    {
        public Task<Product> GetByIdAsync(int id);
        public Task<Product> GetByProductCodeAsync(string productCode);
        public Task<IQueryable<Product>> GetAllAsync(int Pagesize = 10, int pageNumber = 1, bool IsDeleted = false);
        public Task<IQueryable<Product>> GetByCategoryAsync(string category, int Pagesize = 10, int pageNumber = 1, bool IsDeleted = false);
        public Task<Product> CreateAsync(Product product);
        public Task<Product> UpdateAsync(Product product);
        public Task<bool> DeleteAsync(int id);
        public Task<bool> ProductExistsAsync(int id);
        public Task<bool> ProductCodeExistsAsync(string productCode);
        public Task<string> GenerateNextProductCodeAsync(string signature);
        public Task<int> CountAsync();
        public Task<int> CountAsync(Expression<Func<Product, bool>> predicate);
    }
}
