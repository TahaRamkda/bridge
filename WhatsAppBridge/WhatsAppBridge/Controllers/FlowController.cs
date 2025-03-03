using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models;
using WhatsAppBridge.Settings;
using WhatsAppBridge.Models.Integration.Flow;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthenticationSchemes.ApiKeyPolicy)]
    public class FlowController : ControllerBase
    {
        #region Fields

        private readonly ILogger<FlowController> _logger;
        private readonly WhatsAppHandler _whatsAppHandler;

        #endregion

        #region Ctor

        public FlowController(ILogger<FlowController> logger,
            WhatsAppHandler whatsAppHandler)
        {
            _logger = logger;
            _whatsAppHandler = whatsAppHandler;
        }

        #endregion

        #region Methods

        [HttpPost("FlowOps")]
        public async Task<IActionResult> FlowOps([FromBody] CreateFlowRequestDto model)
        {
            _logger.LogInformation("Received FlowOps request with data={data}", JsonConvert.SerializeObject(model));

            if (model == null)
                return Ok(new ApiResult { Message = "Bad Request", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.ClientId))
                return Ok(new ApiResult { Message = "Client Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.SenderNameId))
                return Ok(new ApiResult { Message = "Sender Name Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.Name))
                return Ok(new ApiResult { Message = "Name shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.Category))
                return Ok(new ApiResult { Message = "Category shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.FlowJson))
                return Ok(new ApiResult { Message = "Flow json should be supplied", StatusCode = StatusCodes.Status400BadRequest });

            var resp = await _whatsAppHandler.HandleFlowOps(model);
            return Ok(resp);
        }

        [HttpPost("PublishFlow")]
        public async Task<IActionResult> PublishFlow(PublishFlowRequestDto model)
        {
            _logger.LogInformation("Received PublishFlow request with data={data}", JsonConvert.SerializeObject(model));

            if (model == null)
                return Ok(new ApiResult { Message = "Bad Request", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.ClientId))
                return Ok(new ApiResult { Message = "Client Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.SenderNameId))
                return Ok(new ApiResult { Message = "Sender Name Id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            if (String.IsNullOrWhiteSpace(model.FlowId))
                return Ok(new ApiResult { Message = "Flow id shouldn't be empty", StatusCode = StatusCodes.Status400BadRequest });

            var resp = await _whatsAppHandler.HandlePublishFlow(model);
            return Ok(resp);
        }

        #endregion
    }
}
