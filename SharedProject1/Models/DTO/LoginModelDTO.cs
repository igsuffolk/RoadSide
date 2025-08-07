
using Microsoft.AspNetCore.Identity;

namespace SharedProject1.Models.DTO
{
    public class LoginModelDTO
    {
        public string? UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool LoginFailed { get; set; } = true;
        public List<IdentityError>? Errors { get; set; }

        public string? Token { get; set; } = string.Empty;
    }
}
