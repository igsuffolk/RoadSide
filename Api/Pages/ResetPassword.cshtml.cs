
using ClassLibrary1.Interfaces;
using ClassLibrary1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedProject1.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace Api.Pages
{
    public class ResetPasswordModel : PageModel
    {
        [BindProperty]
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        private readonly IIdentityService _service;

        public ResetPasswordModel(IIdentityService service)
        {
            _service = service;
        }
        public async Task<IActionResult> OnGetAsync(string token, string email)
        {
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string token, string email)
        {
            if (!ModelState.IsValid)
                return Page();

            Console.WriteLine("Reset Token=" + token);

            try
            {
                ResetPasswordDTO model = new()
                {
                    Email = email,
                    Password = Password,
                    Token = token
                };

                IdentityResult loginResult = await _service.ResetPasswordAsync(model);

                Console.WriteLine("token=" + token);

                if (!loginResult.Succeeded)
                {
                    foreach (var error in loginResult.Errors)
                        ModelState.AddModelError(error.Code, error.Description);

                    return Page();
                }

                ModelState.AddModelError("", "Your password has been reset.");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);

            }

            return Page();
        }

    }
}
