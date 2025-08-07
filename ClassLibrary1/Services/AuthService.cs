using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClassLibrary1.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<IdentityService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(ILogger<IdentityService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string> GenerateToken(string authHeader)
        {
            string credentialstring;
            string[] credentials;

            try
            {
                credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader));
                credentials = credentialstring.Split(':');
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }

            if (string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
            {
                _logger.LogError("Empty credentials");
                return null;
            }
            ;

            if (credentials[0] != _configuration.GetSection("Jwt").GetValue<string>("ClientId"))
            {
                _logger.LogError("Invalid ClientId");
                return null;
            }

            if (credentials[1] != _configuration.GetSection("Jwt").GetValue<string>("ClientSecret"))
            {
                _logger.LogError("Invalid ClientSecret");
                return null;
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "angliaweb"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            try
            {
                JwtSecurityToken token = CreateToken(claims);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
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
