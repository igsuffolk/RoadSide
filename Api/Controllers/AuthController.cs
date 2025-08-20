using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
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

            string result = await _authService.GenerateToken(Request.Headers["Authorization"].ToString());

            if (result == null)
                return BadRequest();
            else
                return Ok(result);

        }

    }
}
