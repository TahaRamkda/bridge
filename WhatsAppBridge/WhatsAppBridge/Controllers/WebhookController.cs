using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IOptions<WhatsAppConfigurationSetting> _whatsAppConfigurationSetting;

        public WebhookController(ILogger<WebhookController> logger,
            IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting)
        {
            _logger = logger;
            _whatsAppConfigurationSetting = whatsAppConfigurationSetting;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "hub.mode")] string hubMode = "", [FromQuery(Name = "hub.challenge")] int hubChallenge = 0, [FromQuery(Name = "hub.verify_token")] string hubVerifyToken = "")
        { 
            //var data = JsonSerializer.Serialize(webhookData);
            _logger.LogInformation("Webhook received with request, hub.mode={hubmode}, hub.challenge={hubchallenge},hub.verify_token={verify_token}", hubMode, hubChallenge, hubVerifyToken);

            if (!String.IsNullOrEmpty(hubVerifyToken) && hubChallenge > 0)
            {
                if (hubVerifyToken == _whatsAppConfigurationSetting.Value.WebhookVerificationToken)
                {
                    _logger.LogInformation("Accepted verify webhook request with success with hub.mode={hubmode}, hub.challenge={hubchallenge},hub.verify_token={verify_token}", hubMode, hubChallenge, hubVerifyToken);
                    return Ok(hubChallenge);
                }
                else
                {
                    _logger.LogInformation("Declined verify webhook request with unauthorized with hub.mode={hubmode}, hub.challenge={hubchallenge},hub.verify_token={verify_token}", hubMode, hubChallenge, hubVerifyToken);
                    return Unauthorized("Unauthorized request");
                }
            }


            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post(object payload)
        { 
            var data = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Webhook received with data={data}", data);

            return Ok();

        }
    }
}
