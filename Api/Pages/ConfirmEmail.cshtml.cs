using Api.Helpers;
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages
{
    public class ConfirmEmailModel : PageModel
    {
       
        private readonly IIdentityService _service;

        public string StatusMessage { get; set; }

        public ConfirmEmailModel(IIdentityService service)
        {
            
            _service = service;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(code))
            {
                IdentityResult identityResult = await _service.ConfirmEmailAsync(code,userId);
               
                if (!identityResult.Succeeded)
                {
                    if (identityResult?.Errors?.Count() > 0)
                    {
                        StatusMessage = "Error " + IdentityErrorHelper.ReadErrors(identityResult);
                    }
                    return Page();
                }

                StatusMessage = "Thank you for confirming your email.";

            }
            return Page();
        }
    }
}
