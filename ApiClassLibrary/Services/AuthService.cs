using ApiClassLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiClassLibrary.Services
{
    /// <summary>
    /// Service responsible for creating JWT tokens for clients that authenticate
    /// using HTTP Basic credentials (ClientId:ClientSecret).
    /// </summary>
    public class AuthService : IAuthService
    {
        // Note: the logger is typed for IdentityService in the original code.
        // Preserve as-is to avoid signature changes; it is used for logging here.
        private readonly ILogger<IdentityService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructs the AuthService.
        /// </summary>
        /// <param name="logger">Logger instance (typed for IdentityService in current code).</param>
        /// <param name="configuration">Application configuration (expects Jwt section).</param>
        public AuthService(ILogger<IdentityService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Validates a Basic auth header containing client credentials and, if valid,
        /// generates and returns a signed JWT string. Returns null on failure.
        /// Expected header format: "Basic {base64(ClientId:ClientSecret)}".
        /// </summary>
        /// <param name="authHeader">The full Authorization header value.</param>
        /// <returns>A JWT as string or null if generation failed.</returns>
        public async Task<string?> GenerateToken(string authHeader)
        {
            // Remove "Basic " prefix and trim whitespace.
            authHeader = authHeader.Substring("Basic ".Length).Trim();

            string credentialstring;
            string[] credentials;

            try
            {
                // Decode base64 to "clientId:clientSecret".
                credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader));
                credentials = credentialstring.Split(':');
            }
            catch (Exception ex)
            {
                // Log and return null on malformed base64 or decoding error.
                _logger.LogError(ex.Message, ex);
                return null;
            }

            // Basic validation of decoded components.
            if (string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
            {
                _logger.LogError("Empty credentials");
                return null;
            }

            // Verify client id from configuration.
            if (credentials[0] != _configuration.GetSection("Jwt").GetValue<string>("ClientId"))
            {
                _logger.LogError("Invalid ClientId");
                return null;
            }

            // Verify client secret from configuration.
            if (credentials[1] != _configuration.GetSection("Jwt").GetValue<string>("ClientSecret"))
            {
                _logger.LogError("Invalid ClientSecret");
                return null;
            }

            // Create claims to include in the token. Adjust claims as needed.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "angliaweb"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            try
            {
                // Build the JWT and return the serialized token string.
                JwtSecurityToken token = CreateToken(claims);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log and return null if token creation/signing fails.
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Creates a signed <see cref="JwtSecurityToken"/> using configuration values:
        /// Jwt:SecretKey, Jwt:Issuer, Jwt:Audience and Jwt:ExpiryMinutes.
        /// </summary>
        /// <param name="claims">Claims to embed in the token.</param>
        /// <returns>A signed JwtSecurityToken.</returns>
        private JwtSecurityToken CreateToken(IEnumerable<Claim> claims)
        {
            // Read symmetric key from configuration and create signing credentials.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt").GetValue<string>("SecretKey")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token with issuer, audience, expiration and signing credentials.
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
