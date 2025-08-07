using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedProject1.Models.DTO;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IIdentityService _identityService;

        public IdentityController(ILogger<AuthController> logger, IIdentityService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModelDTO model)
        {
            try
            {
                return Ok(await _identityService.LoginAsync(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            try
            {
                return Ok(await _identityService.RegisterAsync(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("ResendConfirmation/{email}")]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            try
            {
                return Ok(await _identityService.ResendConfirmationAsync(email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("ForgotPassword/{email}")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // user clicked forgot password
            try
            {
                return Ok(await _identityService.ForgotPasswordAsync(email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
