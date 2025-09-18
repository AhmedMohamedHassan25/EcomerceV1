using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Product;
using DTOs.User;
using Mapster;
using Models;

namespace Application.Mapper
{
    public class Mapster
    {
        public static void configure()
        {
            #region mapp userDto
            TypeAdapterConfig<User, UserDTO>.NewConfig()
               .Map(dest => dest.UserName, src => src.UserName)
               .Map(dest => dest.UserId, src => src.Id)
               .Map(dest => dest.Email, src => src.Email)
               .Map(dest => dest.LastLoginTime, src => src.LastLoginTime)
               .Map(dest => dest.CreatedAt, src => src.CreatedAt);
            #endregion

            #region mapp RegisterDto 
                TypeAdapterConfig<RegisterDTO, User>.NewConfig()
                 .Ignore(dest => dest.Id)
                 .Ignore(dest => dest.Password)
                 .Ignore(dest => dest.LastLoginTime)
                 .Ignore(dest => dest.RefreshToken)
                 .Ignore(dest => dest.RefreshTokenExpiry)
                 .Ignore(dest => dest.CreatedAt)
                 .Ignore(dest => dest.UpdatedAt);
            #endregion

            #region mapp LoginDto
            TypeAdapterConfig<LoginDTO, User>
            .NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Email)
            .Ignore(dest => dest.Password)
            .Ignore(dest => dest.LastLoginTime)
            .Ignore(dest => dest.RefreshToken)
            .Ignore(dest => dest.RefreshTokenExpiry)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt);
            #endregion

            #region mapp UpdateUserDto
            TypeAdapterConfig<UpdateUserDTO, User>
            .NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserName)
            .Ignore(dest => dest.Password)
            .Ignore(dest => dest.LastLoginTime)
            .Ignore(dest => dest.RefreshToken)
            .Ignore(dest => dest.RefreshTokenExpiry)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .IgnoreNullValues(true);
            #endregion

            #region mapp ProductDto
            TypeAdapterConfig<Product, ProductDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.ProductCode, src => src.ProductCode)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.ImagePath, src => src.ImagePath)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.MinimumQuantity, src => src.MinimumQuantity)
                .Map(dest => dest.DiscountRate, src => src.DiscountRate)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .PreserveReference(true)
                .TwoWays();
            #endregion

            #region mapp CreateProductDto 
            TypeAdapterConfig<CreateProductDTO, Product>.NewConfig()
               .Ignore(dest => dest.Id)
               .Ignore(dest => dest.ProductCode)
               .Ignore(dest => dest.ImagePath)
               .Ignore(dest => dest.CreatedAt)
               .Ignore(dest => dest.UpdatedAt);

            #endregion

            #region mapp UpdateProductDto  
            TypeAdapterConfig<UpdateProductDTO, Product>.NewConfig()
                    .Ignore(dest => dest.Id)
                    .Ignore(dest => dest.ProductCode)
                    .Ignore(dest => dest.ImagePath)
                    .Ignore(dest => dest.CreatedAt)
                    .Ignore(dest => dest.UpdatedAt)
                    .IgnoreNullValues(true);
            #endregion
        }
    }
}
