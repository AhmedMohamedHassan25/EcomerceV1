using Application.Contract;
using Context;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ProductRepositroy:IProductRepository
    {
        private readonly ECommerceContext _context;

        public ProductRepositroy(ECommerceContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> GetByProductCodeAsync(string productCode)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == productCode);
        }

        public Task<IQueryable<Product>> GetAllAsync(int Pagesize = 0, int pageNumber = 1, bool IsDeleted = false) =>
                       Task.FromResult(
                           _context.Products
                            .Where(b => b.IsDeleted == IsDeleted)
                            .Skip((pageNumber - 1) * Pagesize)
                            .Take(Pagesize)
                            .AsQueryable());

        public  Task<IQueryable<Product>> GetByCategoryAsync(string category, int Pagesize = 0, int pageNumber = 1, bool IsDeleted = false)=>
                       Task.FromResult(
                           _context.Products
                           .Where(p => p.Category.ToLower() == category.ToLower()&& p.IsDeleted==IsDeleted)
                           .OrderBy(p => p.ProductCode)
                           .Skip((pageNumber - 1) * Pagesize)
                           .Take(Pagesize)
                           .AsQueryable());
        

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProductExistsAsync(int id)=>
             await _context.Products.AnyAsync(p => p.Id == id);
        

        public async Task<bool> ProductCodeExistsAsync(string productCode)=>
                await _context.Products.AnyAsync(p => p.ProductCode == productCode);
       

        public async Task<string> GenerateNextProductCodeAsync(string signature)
        {
            var lastProduct = await _context.Products
                .OrderByDescending(p => p.ProductCode)
                .FirstOrDefaultAsync();

            if (lastProduct == null)
                return $"{signature}001";

            var lastCode = lastProduct.ProductCode;
            if (lastCode.Length > 1 && lastCode.StartsWith(signature))
            {
                var numberPart = lastCode.Substring(1);
                if (int.TryParse(numberPart, out int number))
                {
                    return $"{signature}{(number + 1).ToString().PadLeft(3, '0')}";
                }
            }

            return $"{signature}001";
        }

        public virtual async Task<int> CountAsync() => await _context.Users.CountAsync();

        public virtual async Task<int> CountAsync(Expression<Func<Product, bool>> predicate) =>
                await _context.Products.CountAsync(predicate);
    }
}
