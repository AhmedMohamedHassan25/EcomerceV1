using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class CreateProductDTO
    {
        [Required]
        public string Category { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int MinimumQuantity { get; set; }

        [Range(0, 100)]
        public decimal DiscountRate { get; set; } = 0;
    }
}
