using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class TemplateAnalyticsController : Controller
    {
        #region Fields

        private readonly ILogger<TemplateAnalyticsController> _logger;
        private readonly WhatsAppHandler _whatsAppHandler;

        #endregion

        #region Ctor

        public TemplateAnalyticsController(ILogger<TemplateAnalyticsController> logger,
            WhatsAppHandler whatsAppHandler)
        {
            _logger = logger;
            _whatsAppHandler = whatsAppHandler;
        }

        #endregion

        #region Methods

        [HttpPost("TemplateAnalytics")]
        public async Task<IActionResult> TemplateAnalytics([FromBody] TemplateAnalyticsRequestDto model)
        {
            _logger.LogInformation("Received TemplaterAnalytics request with data={data}", JsonConvert.SerializeObject(model));

            if (model == null)
                return Ok(new ApiResult { Message = "Bad Request", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.ClientId))
                return Ok(new ApiResult { Message = "Client Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.SenderId))
                return Ok(new ApiResult { Message = "Sender Name Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });
            if (String.IsNullOrWhiteSpace(model.TemplateId))
                return Ok(new ApiResult { Message = "Template Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (model.StartDate > model.EndDate)
                return Ok(new ApiResult { Message = "Start date shoudn't be greater than end date", StatusCode = StatusCodes.Status400BadRequest });

            var resp = await _whatsAppHandler.FetchTemplateAnalytics(model);
            return Ok(resp);
        }

        #endregion
    }
}
