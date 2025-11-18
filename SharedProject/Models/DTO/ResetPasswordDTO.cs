using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject.Models.DTO
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
