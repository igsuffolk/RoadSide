
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedProject1.Models.DTO;
using SharedProject1.Models.Email;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ClassLibrary1.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _contextAccessor;

        public IdentityService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
           IConfiguration configuration, IWebHostEnvironment hostingEnvironment, IEmailService emailService, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _contextAccessor = contextAccessor;

        }
        public async Task<string> GetUserNameAsync(string userId)
        {
            IdentityUser? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return string.Empty;
            }
            else
            {
                return appUser.UserName;
            }

        }

        public async Task<LoginModelDTO> LoginAsync(LoginModelDTO model)
        {
            IdentityUser? appUser = await _userManager.FindByEmailAsync(model.Email);
            if (appUser == null)
            {
                appUser = await _userManager.FindByNameAsync(model.UserName);
            }

            if (appUser == null)
            {
                model.LoginFailed = true;
                model.Errors = new List<IdentityError>() { new() { Code = "", Description = "Invalid Login Credentials" } };

                return model;
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(appUser, model.Password, false, false);
            if (!signInResult.Succeeded)
            {
                model.LoginFailed = true;

                if (signInResult.IsLockedOut)
                    model.Errors = new List<IdentityError>() { new() { Code = "", Description = "User Locked Out" } };
                else if (signInResult.IsNotAllowed)
                    model.Errors = new List<IdentityError>() { new() { Code = "NotAllowed", Description = "User Not Allowed" } };
                else
                    model.Errors = new List<IdentityError>() { new() { Code = "", Description = "Invalid Login Credentials" } };

                return model;
            }

            // success
            model.UserId = appUser.Id;
            model.LoginFailed = false;
            model.UserName = appUser.UserName;
            model.Email = appUser.Email;

            // create token for client to store in browser
            var claims = new[]
               {
                    new Claim(JwtRegisteredClaimNames.Sub, "RoadSide"),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };


            JwtSecurityToken token = CreateToken(claims);
            model.Token = new JwtSecurityTokenHandler().WriteToken(token);

            return model;
        }
        public async Task<IdentityResult?> ConfirmEmailAsync(string code, string userId)
        {

            IdentityUser? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return new IdentityResult();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, code);

            return identityResult;

        }
        public async Task<bool> ResendConfirmationAsync(string email)
        {

            IdentityUser? appUser = await _userManager.FindByEmailAsync(email);
            if (appUser == null)
                return false;

            if (!await SendConfirmationEmailAsync(appUser))
            {
                return false;
            }

            return true;
        }

        public async Task<IdentityResultDTO?> RegisterAsync(RegisterDTO model)
        {
            IdentityUser user = new()
            {
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            // create user 
            IdentityResult identityResult = await _userManager.CreateAsync(user, model.Password);
            IdentityResultDTO identityResultDTO = new()
            {
                Succeeded = identityResult.Succeeded,
                Errors = identityResult.Errors.ToList()
            };

            if (identityResult.Succeeded)
            {
                // send confirmation email
                await SendConfirmationEmailAsync(user);
            }

            return identityResultDTO;

        }

        public async Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDTO model)
        {
            // called from forgotpassword email

            IdentityUser? appUser = await _userManager.FindByEmailAsync(model.Email);
            if (appUser == null)
            {
                return new IdentityResult();
            }

            model.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

            Console.WriteLine("ResetPassword token=" + model.Token);

            IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, model.Token, model.Password);

            return identityResult;

        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {

            IdentityUser appUser = await _userManager.FindByEmailAsync(email);
            if (appUser == null)
                return false;

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            Console.WriteLine("ForgotPassword token=" + token);

            var link = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}" +
                $"{_contextAccessor.HttpContext.Request.PathBase}/Identity/ResetPassword?token={token}&email={email}";

            string templateFile = File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "Templates", "ResetPasswordEmail.html"));
            templateFile = templateFile.Replace("<#ResetLink#>", link);

            EmailMessage emailMessage = new()
            {
                To = new() { email },
                Subject = "RoadSide Password Reset",
                From = _configuration.GetSection("AppSettings").GetValue<string>("EmailFrom"),
                Content = templateFile
            };

            // emailMessage.EmbeddedResourcesElementPaths.Add(new KeyValuePair<string, string>("<#LogoImage#>", Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "EmailTemplates", "Anglialogo.gif")));

            var content = new StringContent(JsonSerializer.Serialize(emailMessage));

            var requestContent = new MultipartFormDataContent();
            requestContent.Add(content, name: "emailMessage");

            await _emailService.SendEmailAsync(emailMessage);

            return true;
        }
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        private async Task<bool> SendConfirmationEmailAsync(IdentityUser appUser)
        {

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var link = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}" +
                $"{_contextAccessor.HttpContext.Request.PathBase}/Identity/ConfirmEmail?code={code}&userId={appUser.Id}";

            string templateFile = File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "Templates", "RegisterEmail.html"));
            templateFile = templateFile.Replace("<#ConfirmLink#>", link);

            EmailMessage emailMessage = new()
            {
                To = new() { appUser.Email },
                Subject = "RoadSide Email Confirmation",
                From = _configuration.GetSection("AppSettings").GetValue<string>("EmailFrom"),
                Content = templateFile
            };

            //  emailMessage.EmbeddedResourcesElementPaths.Add(new KeyValuePair<string, string>("<#LogoImage#>", Path.Combine(_hostingEnvironment.WebRootPath, "EmailTemplates", "Anglialogo.gif")));

            var content = new StringContent(JsonSerializer.Serialize(emailMessage));

            var requestContent = new MultipartFormDataContent();
            requestContent.Add(content, name: "emailMessage");

            await _emailService.SendEmailAsync(emailMessage);


            return true;
        }
        private JwtSecurityToken CreateToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt").GetValue<string>("SecretKey")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt").GetValue<string>("Issuer"),
                audience: _configuration.GetSection("Jwt").GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetSection("Jwt").GetValue<int>("ExpiryMinutes")),
                signingCredentials: creds
                );

            return token;
        }
    }
}
