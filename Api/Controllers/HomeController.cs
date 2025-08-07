using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedProject1.Models.DTO;
using SharedProject1.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        private readonly ILogger<HomeController> _logger;   
        
        public HomeController(IHomeService homeService, ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> NewReport(RoadReport model)
        {
            try
            {
             return Ok(await _homeService.NewReport(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }       
    }
}
