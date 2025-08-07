using Microsoft.AspNetCore.Identity;
using SharedProject1.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Interfaces
{
    public interface IIdentityService
    {
        Task<IdentityResult?> ConfirmEmailAsync(string code, string userId);
        Task<bool> ForgotPasswordAsync(string email);
        Task<IdentityResultDTO?> RegisterAsync(RegisterDTO model);
        Task<bool> ResendConfirmationAsync(string email);
        Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDTO model);
        Task<LoginModelDTO> LoginAsync(LoginModelDTO model);
        Task LogoutAsync();
        Task<string> GetUserNameAsync(string userId);
    }
}
