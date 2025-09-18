
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Models
{
    public class Product:BaseEntity<int>
     {
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        

        [Required]
        [StringLength(20)]
        public string ProductCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int MinimumQuantity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal DiscountRate { get; set; } = 0;

    }
}
