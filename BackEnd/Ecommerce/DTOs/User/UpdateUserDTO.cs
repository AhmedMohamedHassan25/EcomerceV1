
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UpdateUserDTO
    {
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        [MinLength(6)]
        public string? Password { get; set; }
    }
}
