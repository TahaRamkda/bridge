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
    public class AnalyticsController : ControllerBase
    {
        #region Fields

        private readonly ILogger<AnalyticsController> _logger;
        private readonly WhatsAppHandler _whatsAppHandler;

        #endregion

        #region Ctor

        public AnalyticsController(ILogger<AnalyticsController> logger,
            WhatsAppHandler whatsAppHandler)
        {
            _logger = logger;
            _whatsAppHandler = whatsAppHandler;
        }

        #endregion

        #region Methods

        [HttpPost("ConversationAnalytics")]
        public async Task<IActionResult> ConversationAnalytics([FromBody] ConversationAnalyticsRequestDto model)
        {
            _logger.LogInformation("Received ConversationAnalytics request with data={data}", JsonConvert.SerializeObject(model));

            if (model == null)
                return Ok(new ApiResult { Message = "Bad Request", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.ClientId))
                return Ok(new ApiResult { Message = "Client Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.SenderId))
                return Ok(new ApiResult { Message = "Sender Name Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (model.StartDate > model.EndDate)
                return Ok(new ApiResult { Message = "Start date shoudn't be greater than end date", StatusCode = StatusCodes.Status400BadRequest });

            var resp = await _whatsAppHandler.FetchConversationAnalytics(model);
            return Ok(resp);
        }

        #endregion
    }
}
