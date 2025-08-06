using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharedProject1.Models.DTO
{
    public class LoginDTO
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Email { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
        public bool LoginResult { get; set; } = false;
    }
}
