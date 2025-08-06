using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedProject1.Models;
using SharedProject1.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public HomeController(IEmailService emailService, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (model.Email == "iaingrant80@googlemail.com" && model.Password == "testIaing")
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "RoadSide"),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                try
                {
                    JwtSecurityToken token = CreateToken(claims);

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                catch (Exception ex)
                {
                    return BadRequest("Token Error " + ex.Message);
                }
               
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewReport(RoadReport model)
        {
            try
            {
                string fileName = Guid.NewGuid().ToString() + ".jpg";

                EmailMessage message = new()
                {
                    To = new List<string> { "iaingrant80@googlemail.com" },
                    Subject = "Roadside Report"
                };

                List<KeyValuePair<string, string>> listDetails = new();
                listDetails.Add(new KeyValuePair<string, string>("<#ReportDate#>", model.ReportDate.ToString("dd/MMM/yyyy")));
                listDetails.Add(new KeyValuePair<string, string>("<#ReportBy#>", model.ReportedBy));
                listDetails.Add(new KeyValuePair<string, string>("<#RoadName#>", model.RoadName));
                listDetails.Add(new KeyValuePair<string, string>("<#Description#>", model.Description));
                listDetails.Add(new KeyValuePair<string, string>("<#Latitude#>", model.Latitude.ToString()));
                listDetails.Add(new KeyValuePair<string, string>("<#Longitude#>", model.Longitude.ToString()));

                message.EmbeddedResourcesElementPaths = listDetails;

                if (model.Photo != null)
                {
                    message.Attachments = new List<string> { fileName };

                    byte[] photo = model.Photo;

                    System.IO.File.WriteAllBytes(Path.Combine(_webHostEnvironment.WebRootPath, "temp", fileName), photo);
                }

                await _emailService.SendEmailAsync(message);

                return Ok(true);
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

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
