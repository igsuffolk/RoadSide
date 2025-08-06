using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IConfiguration configuration, ILogger<AuthController> logger  )
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("Ping")]
        public async Task<IActionResult> Ping()
        {
            return Ok();
        }

        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return BadRequest("Missing Authorization header");
            }

            string authHeader = Request.Headers["Authorization"].ToString();
            authHeader = authHeader.Substring("Basic ".Length).Trim();
            string credentialstring;
            string[] credentials;

            try
            {
                credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader));
                credentials = credentialstring.Split(':');
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid Credentials");
            }

            if (string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
            {
                return BadRequest();
            }
            ;

            if (credentials[0] != _configuration.GetSection("Jwt").GetValue<string>("ClientId"))
            {
                return BadRequest("Invalid Credentials");
            }

            if (credentials[1] != _configuration.GetSection("Jwt").GetValue<string>("ClientSecret"))
            {
                return BadRequest("Invalid Credentials");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "angliaweb"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            try
            {
                JwtSecurityToken token = CreateToken(claims);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                _logger.LogError("", ex);

                return BadRequest("Token Error " + ex.Message);
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
