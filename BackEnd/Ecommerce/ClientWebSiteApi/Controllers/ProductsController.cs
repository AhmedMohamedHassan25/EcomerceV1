using Application.Services.ProductServices;
using DTOs.Product;
using DTOs.SharedData;
using Handmade.DTOs.SharedDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace ClientWebSiteApi.Controllers
{
  
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class ProductsController : ControllerBase
        {
            private readonly IProductService _productService;

            public ProductsController(IProductService productService)
            {
                _productService = productService;
            }

            [HttpGet]
            public async Task<ActionResult<Result<EntityPaginated<ProductDTO>>>> GetProducts()
            {
                var products = await _productService.GetAllAsync();
                return Ok(Result<EntityPaginated<ProductDTO>>.Success(products));
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Result<ProductDTO>>> GetProduct(int id)
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(Result<ProductDTO>.Failure("Product not found"));
                }
                return Ok(Result<ProductDTO>.Success(product));
            }

            [HttpGet("category/{category}")]
            public async Task<ActionResult<Result<EntityPaginated<ProductDTO>>>> GetProductsByCategory(string category)
            {
                var products = await _productService.GetByCategoryAsync(category);
                    if (products == null)
                    {
                        return NotFound(Result<ProductDTO>.Failure("no Products in this category"));
                    }
            return Ok(Result<EntityPaginated<ProductDTO>>.Success(products));
            }

            [HttpPost]
            public async Task<ActionResult<Result<ProductDTO>>> CreateProduct([FromBody] CreateProductDTO createProductDto)
            {
                try
                {
                    var product = await _productService.CreateAsync(createProductDto);
                    return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId },
                        Result<ProductDTO>.Success(product));
                }
                catch (Exception ex)
                {
                    return BadRequest(Result<ProductDTO>.Failure(ex.Message));
                }
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<Result<ProductDTO>>> UpdateProduct(int id, [FromBody] UpdateProductDTO updateProductDto)
            {
                try
                {
                    var product = await _productService.UpdateAsync(id, updateProductDto);
                    if (product == null)
                    {
                        return NotFound(Result<ProductDTO>.Failure("Product not found"));
                    }
                    return Ok(Result<ProductDTO>.Success(product));
                }
                catch (Exception ex)
                {
                    return BadRequest(Result<ProductDTO>.Failure(ex.Message));
                }
            }

            [HttpDelete("{id}")]
            public async Task<ActionResult<Result>> DeleteProduct(int id)
            {
                var deleted = await _productService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(Result.Failure("Product not found"));
                }
                return Ok(Result.Success());
            }

            [HttpPost("{id}/image")]
            public async Task<ActionResult<Result<string>>> UploadProductImage(int id, IFormFile file)
            {
                try
                {
                    if (file == null || file.Length == 0)
                    {
                        return BadRequest(Result<string>.Failure("No file uploaded"));
                    }

                    var imagePath = await _productService.UploadImageAsync(id, file);
                    return Ok(Result<string>.Success(imagePath));
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(Result<string>.Failure(ex.Message));
                }
                catch
                {
                    return StatusCode(500, Result<string>.Failure("Failed to upload image"));
                }
            }
        }
 }
