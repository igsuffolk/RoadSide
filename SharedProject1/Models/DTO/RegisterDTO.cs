using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject1.Models.DTO
{
    public class RegisterDTO
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
