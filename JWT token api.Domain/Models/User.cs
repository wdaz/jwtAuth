using System;
using System.Collections.Generic;
using System.Text;

namespace JWT_token_api.Domain.Models
{
    public class User
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; }
    }
}
