using Application.Contract;
using Application.Services.FileServices;
using DTOs.Product;
using DTOs.SharedData;
using Handmade.DTOs.SharedDTOs;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ProductServices
{
    public class ProductService(IProductRepository _productRepository, IFileService _fileService, ILogger<Product> _logger) : IProductService
    {

        public async Task<ProductDTO> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting product with ID: {ProductId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID provided: {ProductId}", id);
                    throw new ArgumentException("Product ID must be greater than 0", nameof(id));
                }

                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogInformation("Product with ID {ProductId} not found", id);
                    return null;
                }

                var productDto = product.Adapt<ProductDTO>();
                _logger.LogInformation("Successfully retrieved product with ID: {ProductId}", id);
                return productDto;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID: {ProductId}", id);
                throw new InvalidOperationException("An error occurred while retrieving the product", ex);
            }
        }


        public async Task<EntityPaginated<ProductDTO>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool isDeleted = false)
        {
            try
            {
                _logger.LogInformation("Getting products - Page: {PageNumber}, Size: {PageSize}, IsDeleted: {IsDeleted}",
                    pageNumber, pageSize, isDeleted);

                if (pageNumber < 1)
                {
                    _logger.LogWarning("Invalid page number provided: {PageNumber}, returning empty result", pageNumber);
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                if (pageSize < 1)
                {
                    _logger.LogWarning("Invalid page size provided: {PageSize}, returning empty result", pageSize);
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                var getProductsTask = _productRepository.GetAllAsync(pageSize, pageNumber, isDeleted);
                var getTotalCountTask = _productRepository.CountAsync(b => b.IsDeleted == isDeleted);
                await Task.WhenAll(getProductsTask, getTotalCountTask);
                var products = await getProductsTask;
                var totalCount = await getTotalCountTask;

                _logger.LogInformation("Retrieved {ProductCount} products out of {TotalCount} total products",
                    products.Count(), totalCount);

                if (!products.Any())
                {
                    _logger.LogInformation("No products found for the specified criteria");
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    };
                }

                var productDtos = products.Adapt<IEnumerable<ProductDTO>>();
                var entityPaginated = new EntityPaginated<ProductDTO>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                _logger.LogInformation("Successfully returned paginated products - Page {PageNumber}, " +
                    "Items: {ItemCount}",
                    pageNumber, productDtos.Count());

                return entityPaginated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products - Page: {PageNumber}, " +
                    "Size: {PageSize}, IsDeleted: {IsDeleted}, returning empty result", pageNumber, pageSize, isDeleted);

                return new EntityPaginated<ProductDTO>
                {
                    Items = Enumerable.Empty<ProductDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }


        public async Task<EntityPaginated<ProductDTO>> GetByCategoryAsync(string category, int pageNumber = 1, int pageSize = 10, bool isDeleted = false)
        {
            try
            {
                _logger.LogInformation("Getting products by category: {Category} - Page: {PageNumber}, Size: {PageSize}",
                    category, pageNumber, pageSize);

                if (string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogWarning("Category is null or empty, returning empty result");
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                if (pageNumber < 1)
                {
                    _logger.LogWarning("Invalid page number provided: {PageNumber} for category: {Category}, returning empty result",
                        pageNumber, category);
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                if (pageSize < 1)
                {
                    _logger.LogWarning("Invalid page size provided: {PageSize} for category: {Category}, returning empty result",
                        pageSize, category);
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }


                var getProductsTask = _productRepository.GetByCategoryAsync(category, pageSize, pageNumber, isDeleted);
                var getTotalCountTask = _productRepository.CountAsync(p => p.Category.ToLower() == category.ToLower() && p.IsDeleted == isDeleted);

                await Task.WhenAll(getProductsTask, getTotalCountTask);

                var products = await getProductsTask;
                var totalCount = await getTotalCountTask;

                _logger.LogInformation("Retrieved {ProductCount} products out of {TotalCount} total products for category: {Category}",
                    products.Count(), totalCount, category);

                if (!products.Any())
                {
                    _logger.LogInformation("No products found for category: {Category}", category);
                    return new EntityPaginated<ProductDTO>
                    {
                        Items = Enumerable.Empty<ProductDTO>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    };
                }

                var productDtos = products.Adapt<IEnumerable<ProductDTO>>();
                var entityPaginated = new EntityPaginated<ProductDTO>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                _logger.LogInformation("Successfully returned paginated products for category: {Category} - Page {PageNumber}, " +
                    "Items: {ItemCount}", category, pageNumber, productDtos.Count());

                return entityPaginated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products by category: {Category} - Page: {PageNumber}, " +
                    "Size: {PageSize}, returning empty result", category, pageNumber, pageSize);

                return new EntityPaginated<ProductDTO>
                {
                    Items = Enumerable.Empty<ProductDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        public async Task<ProductDTO> CreateAsync(CreateProductDTO createProductDto)
        {
            try
            {
                _logger.LogInformation("Creating new product with name: {ProductName}", createProductDto?.Name);

                if (createProductDto == null)
                {
                    _logger.LogWarning("CreateProductDTO is null, returning null");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(createProductDto.Name))
                {
                    _logger.LogWarning("Product name is null or empty, returning null");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(createProductDto.Category))
                {
                    _logger.LogWarning("Product category is null or empty, returning null");
                    return null;
                }

                if (createProductDto.Price <= 0)
                {
                    _logger.LogWarning("Invalid product price: {Price}, returning null", createProductDto.Price);
                    return null;
                }

                if (createProductDto.MinimumQuantity < 0)
                {
                    _logger.LogWarning("Invalid minimum quantity: {MinimumQuantity}, returning null", createProductDto.MinimumQuantity);
                    return null;
                }

                if (createProductDto.DiscountRate < 0 || createProductDto.DiscountRate > 100)
                {
                    _logger.LogWarning("Invalid discount rate: {DiscountRate}, returning null", createProductDto.DiscountRate);
                    return null;
                }

                var productCode = await _productRepository.GenerateNextProductCodeAsync("P");
                var product = new Product
                {
                    Category = createProductDto.Category,
                    ProductCode = productCode,
                    Name = createProductDto.Name,
                    Price = createProductDto.Price,
                    MinimumQuantity = createProductDto.MinimumQuantity,
                    DiscountRate = createProductDto.DiscountRate
                };

                product = await _productRepository.CreateAsync(product);
                var productDto = product.Adapt<ProductDTO>();

                _logger.LogInformation("Successfully created product with ID: {ProductId} and code: {ProductCode}",
                    product.Id, productCode);

                return productDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product with name: {ProductName}, returning null",
                    createProductDto?.Name);
                return null;
            }
        }
        public async Task<ProductDTO> UpdateAsync(int id, UpdateProductDTO updateProductDto)
        {
            try
            {
                _logger.LogInformation("Updating product with ID: {ProductId}", id);


                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID provided: {ProductId}, returning null", id);
                    return null;
                }

                if (updateProductDto == null)
                {
                    _logger.LogWarning("UpdateProductDTO is null for product ID: {ProductId}, returning null", id);
                    return null;
                }

                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogInformation("Product with ID {ProductId} not found for update, returning null", id);
                    return null;
                }

                if (!string.IsNullOrEmpty(updateProductDto.Category))
                {
                    if (string.IsNullOrWhiteSpace(updateProductDto.Category))
                    {
                        _logger.LogWarning("Empty category provided for product ID: {ProductId}, returning null", id);
                        return null;
                    }
                    product.Category = updateProductDto.Category;
                }

                if (!string.IsNullOrEmpty(updateProductDto.Name))
                {
                    if (string.IsNullOrWhiteSpace(updateProductDto.Name))
                    {
                        _logger.LogWarning("Empty name provided for product ID: {ProductId}, returning null", id);
                        return null;
                    }
                    product.Name = updateProductDto.Name;
                }

                if (updateProductDto.Price.HasValue)
                {
                    if (updateProductDto.Price.Value <= 0)
                    {
                        _logger.LogWarning("Invalid price {Price} provided for product ID: {ProductId}, returning null",
                            updateProductDto.Price.Value, id);
                        return null;
                    }
                    product.Price = updateProductDto.Price.Value;
                }

                if (updateProductDto.MinimumQuantity.HasValue)
                {
                    if (updateProductDto.MinimumQuantity.Value < 0)
                    {
                        _logger.LogWarning("Invalid minimum quantity {MinimumQuantity} provided for product ID: {ProductId}, returning null",
                            updateProductDto.MinimumQuantity.Value, id);
                        return null;
                    }
                    product.MinimumQuantity = updateProductDto.MinimumQuantity.Value;
                }

                if (updateProductDto.DiscountRate.HasValue)
                {
                    if (updateProductDto.DiscountRate.Value < 0 || updateProductDto.DiscountRate.Value > 100)
                    {
                        _logger.LogWarning("Invalid discount rate {DiscountRate} provided for product ID: {ProductId}, returning null",
                            updateProductDto.DiscountRate.Value, id);
                        return null;
                    }
                    product.DiscountRate = updateProductDto.DiscountRate.Value;
                }

                product = await _productRepository.UpdateAsync(product);
                var productDto = product.Adapt<ProductDTO>();

                _logger.LogInformation("Successfully updated product with ID: {ProductId}", id);
                return productDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID: {ProductId}, returning null", id);
                return null;
            }
        }


        public async Task<bool> DeleteAsync(int id, bool soft)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID: {ProductId}, Soft delete: {SoftDelete}", id, soft);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID provided for deletion: {ProductId}, returning false", id);
                    return false;
                }

                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogInformation("Product with ID {ProductId} not found for deletion, returning false", id);
                    return false;
                }

                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    try
                    {
                        await _fileService.DeleteFileAsync(product.ImagePath);
                        _logger.LogInformation("Successfully deleted image for product ID: {ProductId}, Path: {ImagePath}",
                            id, product.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image for product ID: {ProductId}, Path: {ImagePath}",
                            id, product.ImagePath);
                    }
                }

                bool result;
                if (soft)
                {
                    product.IsDeleted = true;
                    await _productRepository.UpdateAsync(product);
                    result = true;
                    _logger.LogInformation("Successfully soft deleted product with ID: {ProductId}", id);
                }
                else
                {
                    result = await _productRepository.DeleteAsync(id);
                    _logger.LogInformation("Successfully hard deleted product with ID: {ProductId}, Result: {Result}",
                        id, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID: {ProductId}, returning false", id);
                return false;
            }
        }
        public async Task<string> UploadImageAsync(int id, IFormFile file)
        {
            try
            {
                _logger.LogInformation("Uploading image for product with ID: {ProductId}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID provided for image upload: {ProductId}, returning null", id);
                    return null;
                }

                if (file == null)
                {
                    _logger.LogWarning("No file provided for product ID: {ProductId}, returning null", id);
                    return null;
                }

                if (file.Length == 0)
                {
                    _logger.LogWarning("Empty file provided for product ID: {ProductId}, returning null", id);
                    return null;
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Invalid file type {FileExtension} for product ID: {ProductId}, returning null",
                        fileExtension, id);
                    return null;
                }

                const long maxFileSize = 5 * 1024 * 1024; 
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning("File size {FileSize} bytes exceeds limit for product ID: {ProductId}, returning null",
                        file.Length, id);
                    return null;
                }

                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found for image upload, ID: {ProductId}, returning null", id);
                    return null;
                }

                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    try
                    {
                        await _fileService.DeleteFileAsync(product.ImagePath);
                        _logger.LogInformation("Successfully deleted existing image for product ID: {ProductId}, Path: {ImagePath}",
                            id, product.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete existing image for product ID: {ProductId}, Path: {ImagePath}",
                            id, product.ImagePath);
                    }
                }

                var imagePath = await _fileService.SaveFileAsync(file, "products");
                product.ImagePath = imagePath;
                await _productRepository.UpdateAsync(product);

                _logger.LogInformation("Successfully uploaded image for product ID: {ProductId}, Path: {ImagePath}",
                    id, imagePath);

                return imagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading image for product ID: {ProductId}, returning null", id);
                return null;
            }
        }
    }
}

