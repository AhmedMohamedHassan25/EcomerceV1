
using System.ComponentModel.DataAnnotations;


namespace DTOs.User
{
    public class LoginDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
