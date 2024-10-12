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
    [AllowAnonymous]
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


        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto model)
        {
            _logger.LogInformation("Facebook webhook received with data={data}", JsonConvert.SerializeObject(model));

            if (model == null)
            {
                return BadRequest(new ApiResult
                {
                    Message = "Bad Request",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.PhoneId))
            {
                return BadRequest(new ApiResult
                {
                    Message = "Message shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.Type))
            {
                return BadRequest(new ApiResult
                {
                    Message = "Type shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (model.Type == MessageType.TEXT && String.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest(new ApiResult
                {
                    Message = "Message shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (!model.PhoneNumbers.Any())
            {
                return BadRequest(new ApiResult
                {
                    Message = "Please enter phone number(s)",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _whatsAppHandler.HandleSendMessage(model);
            if (response == null || response.Count == 0)
            {
                return Ok(new ApiResult
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "Something went wrong"
                });
            }

            return Ok(new ApiResult
            {
                StatusCode = 200,
                Message = "Message sent successfully",
                Result = response
            });
        }
    }
}
