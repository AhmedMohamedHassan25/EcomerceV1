using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class ProductDTO
    {
        public int ProductId { get; set; }

        public string Category { get; set; }

        public string ProductCode { get; set; }

        public string Name { get; set; }

        public string? ImagePath { get; set; }

        public decimal Price { get; set; }

        public int MinimumQuantity { get; set; }

        public decimal DiscountRate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
