using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Context
{
    public static class DbInitializer
    {
        public static void Seed(ECommerceContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    UserName = "Ahmedhassan25",
                    Email = "ahmedmohamedhassan2512@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("elghoul55"),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                context.Products.Add(
                                 new Product
                                 {
                                     Category = "Electronics",
                                     ProductCode = "P001",
                                     Name = "Laptop",
                                     Price = 999.99m,
                                     MinimumQuantity = 5,
                                     DiscountRate = 10.0m,
                                     CreatedAt = new DateTime(2025, 1, 1),
                                     UpdatedAt = new DateTime(2025, 1, 1)
                                 });  
                context.Products.Add(
                                 new Product
                                 {
                                     Category = "Electronics",
                                     ProductCode = "P002",
                                     Name = "Smartphone",
                                     Price = 699.99m,
                                     MinimumQuantity = 10,
                                     DiscountRate = 5.0m,
                                     CreatedAt = new DateTime(2025, 1, 1),
                                     UpdatedAt = new DateTime(2025, 1, 1)
                                 } );
                context.SaveChanges();
            }
        }
    }

}
