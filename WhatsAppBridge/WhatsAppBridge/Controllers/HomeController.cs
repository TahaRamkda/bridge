using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppBridge.Handler;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IntegrationHandler _integrationHandler;

        public HomeController(ILogger<HomeController> logger,
            IntegrationHandler integrationHandler)
        {
            _logger = logger;
            _integrationHandler = integrationHandler;
        }

        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var a = await _integrationHandler.GetClientInformation("1");
            return Ok(a);
        }
    }
}
