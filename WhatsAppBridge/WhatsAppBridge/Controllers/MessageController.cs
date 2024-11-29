using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.WhatsApp.Types;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthenticationSchemes.ApiKeyPolicy)]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly WhatsAppHandler _whatsAppHandler;

        public MessageController(ILogger<MessageController> logger,
            IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
            WhatsAppHandler whatsAppHandler)
        {
            _logger = logger;
            _whatsAppHandler = whatsAppHandler;
        }

        [HttpPost("SendBatchMessage")]
        public async Task<IActionResult> SendBatchMessage([FromBody] SendMessageRequestDto model)
        {
            _logger.LogInformation("Received SendBatchMessage request with data={data}", JsonConvert.SerializeObject(model));

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

            if (String.IsNullOrWhiteSpace(model.Type))
            {
                return Ok(new ApiResult
                {
                    Message = "Type shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (model.Type == MessageType.TEXT && String.IsNullOrWhiteSpace(model.Message))
            {
                return Ok(new ApiResult
                {
                    Message = "Message shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (!model.PhoneNumbers.Any())
            {
                return Ok(new ApiResult
                {
                    Message = "Please enter phone number(s)",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _whatsAppHandler.HandleSendBatchMessage(model);
            return Ok(response);
        }

        [HttpPost("SendInteractiveMessage")]
        public async Task<IActionResult> SendInteractiveMessage([FromBody] SendInteractiveMessageRequestDto model)
        {
            _logger.LogInformation("Received SendInteractiveMessage request with data={data}", JsonConvert.SerializeObject(model));

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

            if (!model.Buttons.Any())
            {
                return Ok(new ApiResult
                {
                    Message = "Buttons should be available",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (!model.PhoneNumbers.Any())
            {
                return Ok(new ApiResult
                {
                    Message = "Please enter phone number(s)",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _whatsAppHandler.HandleSendInteractiveMessage(model);
            return Ok(response);
        }
    }
}
