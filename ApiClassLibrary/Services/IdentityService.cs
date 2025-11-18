using ApiClassLibrary.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedProject.Models.DTO;
using SharedProject.Models.Email;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ApiClassLibrary.Services
{
    /// <summary>
    /// Provides user identity operations: registration, login, email confirmation,
    /// password reset, and related email notifications.
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes a new instance of <see cref="IdentityService"/>.
        /// </summary>
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

        /// <summary>
        /// Returns the username for the specified user id, or an empty string if not found.
        /// </summary>
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

        /// <summary>
        /// Attempts to sign a user in by email (or username) and password.
        /// On success returns a <see cref="LoginModelDTO"/> containing a JWT token and user details.
        /// On failure the returned DTO indicates failure and contains identity errors.
        /// </summary>
        public async Task<LoginDTO> LoginAsync(LoginDTO model)
        {
            // Try to find the user by email first, then by username.
            IdentityUser? appUser = await _userManager.FindByEmailAsync(model.Email);
            if (appUser == null)
            {
                appUser = await _userManager.FindByNameAsync(model.Email);
            }

            // If user not found, mark login failed and return an error.
            if (appUser == null)
            {
                model.LoginFailed = true;
                model.Errors = new List<IdentityError>() { new() { Code = "", Description = "Invalid Login Credentials" } };
                return model;
            }

            // Attempt password sign-in (not persistent, not lockout on failure here).
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

            // On success populate DTO with user info.
            model.UserId = appUser.Id;
            model.LoginFailed = false;
            model.UserName = appUser.UserName;
            model.Email = appUser.Email;

            // Build claims for the JWT to return to the client.
            var claims = new[]
               {
                    new Claim(JwtRegisteredClaimNames.Sub, "RoadSide"),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            // Create signed JWT and attach to model.
            JwtSecurityToken token = CreateToken(claims);
            model.Token = new JwtSecurityTokenHandler().WriteToken(token);

            return model;
        }

        /// <summary>
        /// Confirms a user's email using a URL-encoded token and user id.
        /// Returns the IdentityResult from UserManager.
        /// </summary>
        public async Task<IdentityResult?> ConfirmEmailAsync(string code, string userId)
        {
            IdentityUser? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return new IdentityResult();
            }

            // Decode token from Base64 URL encoding before passing to UserManager.
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, code);

            return identityResult;
        }

        /// <summary>
        /// Resends the confirmation email for the specified address if the user exists.
        /// </summary>
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

        /// <summary>
        /// Registers a new user using the provided model and sends a confirmation email on success.
        /// Returns an IdentityResultDTO indicating success/errors.
        /// </summary>
        public async Task<IdentityResultDTO?> RegisterAsync(RegisterDTO model)
        {
            IdentityUser user = new()
            {
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            // Create user with provided password.
            IdentityResult identityResult = await _userManager.CreateAsync(user, model.Password);
            IdentityResultDTO identityResultDTO = new()
            {
                Succeeded = identityResult.Succeeded,
                Errors = identityResult.Errors.ToList()
            };

            // If creation succeeded, send confirmation email.
            if (identityResult.Succeeded)
            {
                await SendConfirmationEmailAsync(user);
            }

            return identityResultDTO;
        }

        /// <summary>
        /// Resets a user's password using a token provided from the forgot-password workflow.
        /// </summary>
        public async Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDTO model)
        {
            // Find user by email.
            IdentityUser? appUser = await _userManager.FindByEmailAsync(model.Email);
            if (appUser == null)
            {
                return new IdentityResult();
            }

            // Decode token from Base64 URL encoding before using it.
            model.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

            Console.WriteLine("ResetPassword token=" + model.Token);

            IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, model.Token, model.Password);

            return identityResult;
        }

        /// <summary>
        /// Initiates the forgot-password flow by generating a password reset token and emailing the user a link.
        /// </summary>
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            IdentityUser? appUser = await _userManager.FindByEmailAsync(email);
            if (appUser == null)
                return false;

            // Generate reset token and URL-safe encode it for use in links.
            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            Console.WriteLine("ForgotPassword token=" + token);

            // Build reset link using current request context (scheme, host, path base).
            var link = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}" +
                $"{_contextAccessor.HttpContext.Request.PathBase}/Identity/ResetPassword?token={token}&email={email}";

            // Load email template and insert generated link.
            string templateFile = File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "Templates", "ResetPasswordEmail.html"));
            templateFile = templateFile.Replace("<#ResetLink#>", link);

            EmailMessage emailMessage = new()
            {
                To = new() { email },
                Subject = "RoadSide Password Reset",
                From = _configuration.GetSection("AppSettings").GetValue<string>("EmailFrom"),
                Content = templateFile
            };

            // Serialize and send the email via injected email service.
            var content = new StringContent(JsonSerializer.Serialize(emailMessage));
            var requestContent = new MultipartFormDataContent();
            requestContent.Add(content, name: "emailMessage");

            await _emailService.SendEmailAsync(emailMessage);

            return true;
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        /// <summary>
        /// Generates and sends an email confirmation message containing a URL-safe encoded token.
        /// Returns true if the send operation was initiated.
        /// </summary>
        private async Task<bool> SendConfirmationEmailAsync(IdentityUser appUser)
        {
            // Generate the confirmation token and encode it for URL use.
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Build the confirmation link using the current request context.
            var link = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}" +
                $"{_contextAccessor.HttpContext.Request.PathBase}/Identity/ConfirmEmail?code={code}&userId={appUser.Id}";

            // Load template and insert the confirmation link.
            string templateFile = File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "Templates", "RegisterEmail.html"));
            templateFile = templateFile.Replace("<#ConfirmLink#>", link);

            EmailMessage emailMessage = new()
            {
                To = new() { appUser.Email },
                Subject = "RoadSide Email Confirmation",
                From = _configuration.GetSection("AppSettings").GetValue<string>("EmailFrom"),
                Content = templateFile
            };

            var content = new StringContent(JsonSerializer.Serialize(emailMessage));
            var requestContent = new MultipartFormDataContent();
            requestContent.Add(content, name: "emailMessage");

            await _emailService.SendEmailAsync(emailMessage);

            return true;
        }

        /// <summary>
        /// Creates a signed JWT using the configured secret, issuer, audience and expiry.
        /// </summary>
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
