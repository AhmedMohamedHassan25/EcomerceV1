using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        //public string Rule { get; set; } = "normal_user";
        public string Email { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
