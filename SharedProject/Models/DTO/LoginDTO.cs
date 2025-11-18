
using Microsoft.AspNetCore.Identity;

namespace SharedProject.Models.DTO
{
    public class LoginDTO
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
