using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class UpdateProductDTO
    {
        public string? Category { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? MinimumQuantity { get; set; }

        [Range(0, 100)]
        public decimal? DiscountRate { get; set; }
    }
}
