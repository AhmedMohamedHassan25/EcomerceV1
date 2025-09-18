using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class RegisterDTO
    {
        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100), MinLength(6)]
        public string Password { get; set; }

        [Required,EmailAddress]
        public string Email { get; set; }
    }
}
