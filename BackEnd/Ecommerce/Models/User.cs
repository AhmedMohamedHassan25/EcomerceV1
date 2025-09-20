
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User:BaseEntity<int>
    {


        [Required]
        [StringLength(50)]
        public string UserName { get; set; }


        [Required]
        [MinLength(8)]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        //public string  Rule{ get; set; }= "normal_user";

        public DateTime? LastLoginTime { get; set; }

        [StringLength(256)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

       
    }
}
