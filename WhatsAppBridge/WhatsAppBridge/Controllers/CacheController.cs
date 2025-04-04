using Microsoft.AspNetCore.Mvc;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Services;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        #region Fields

        private readonly ILogger<CacheController> _logger;
        private readonly ICacheService _cacheService;

        #endregion

        #region 

        public CacheController(ICacheService cacheService,
            ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        #endregion

        [HttpPost("Clear")]
        public IActionResult Clear()
        {
            _logger.LogInformation("Clearing all the caching");

            _cacheService.Clear();
            return Ok("Cache cleared!");
        }

        [HttpPost("ClearByPrefix")]
        public IActionResult ClearByPrefix(string prefix)
        {
            _logger.LogInformation("Clearing all the caching related to prefix={prefix}", prefix);

            _cacheService.RemoveByPrefix(prefix);
            return Ok($"Cache cleared related to prefix - {prefix}");
        }
    }
}
