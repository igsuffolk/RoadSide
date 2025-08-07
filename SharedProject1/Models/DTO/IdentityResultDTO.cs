using Microsoft.AspNetCore.Identity;

namespace SharedProject1.Models.DTO
{
    public class IdentityResultDTO
    {
        public bool Succeeded { get; set; } = false;
        public List<IdentityError>? Errors { get; set; }

    }
}
