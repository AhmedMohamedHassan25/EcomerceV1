using DTOs.Product;
using Handmade.DTOs.SharedDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ProductServices
{
    public interface IProductService
    {

            public Task<ProductDTO> GetByIdAsync(int id);
            public Task<EntityPaginated<ProductDTO>> GetAllAsync(int pageNumber=1,int pageSize=10, bool IsDeleted = false);
            public Task<EntityPaginated<ProductDTO>> GetByCategoryAsync(string category, int pageNumber = 1, int pageSize = 10, bool IsDeleted= false);
            public Task<ProductDTO> CreateAsync(CreateProductDTO createProductDto);
            public Task<ProductDTO> UpdateAsync(int id, UpdateProductDTO updateProductDto);
            public Task<bool> DeleteAsync(int id,bool soft=true);
            public Task<string> UploadImageAsync(int id, IFormFile file);
        
    }
}
