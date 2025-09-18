using DTOs.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SharedData
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
        public UserDTO User { get; set; }
    }
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
