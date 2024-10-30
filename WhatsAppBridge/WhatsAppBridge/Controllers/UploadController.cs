using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthenticationSchemes.ApiKeyPolicy)]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly WhatsAppHandler _whatsAppHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadController(ILogger<UploadController> logger,
            IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
            WhatsAppHandler whatsAppHandler,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _whatsAppHandler = whatsAppHandler;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("UploadMedia")]
        public async Task<IActionResult> UploadMedia([FromBody] UploadMediaDto model)
        {
            _logger.LogInformation("Upload Media payload received with data={data}", JsonConvert.SerializeObject(model));

            if (model == null)
            {
                return Ok(new ApiResult
                {
                    Message = "Bad Request",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.ClientId))
            {
                return Ok(new ApiResult
                {
                    Message = "Client id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.SenderNameId))
            {
                return Ok(new ApiResult
                {
                    Message = "Sender Name Id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (model.Medias == null || !model.Medias.Any())
            {
                return Ok(new ApiResult
                {
                    Message = "Please upload media",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var results = await _whatsAppHandler.HandleMediaUpload(model);
            return Ok(results);
        }
    }
}
