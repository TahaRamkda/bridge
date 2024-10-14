using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Models.WhatsApp.Webhook;
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
        private readonly WhatsAppWebhookHandler _whatsAppWebhookHandler;

        public WebhookController(ILogger<WebhookController> logger,
            IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
            WhatsAppWebhookHandler whatsAppWebhookHandler)
        {
            _logger = logger;
            _whatsAppConfigurationSetting = whatsAppConfigurationSetting;
            _whatsAppWebhookHandler = whatsAppWebhookHandler;
        }

        [HttpGet]
        public ActionResult Get([FromQuery(Name = "hub.mode")] string hubMode = "", [FromQuery(Name = "hub.challenge")] int hubChallenge = 0, [FromQuery(Name = "hub.verify_token")] string hubVerifyToken = "")
        { 
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
            var data = System.Text.Json.JsonSerializer.Serialize(payload);

            _logger.LogInformation("Facebook webhook received with data={data}", data);

            var model = JsonConvert.DeserializeObject<WhatsAppWebhookModel>(data);
            if (model == null)
                return BadRequest("Cannot parse received facebook webhook data object");

            foreach (var entry in model.entry)
            {
                foreach (var change in entry.changes)
                {
                    if (String.IsNullOrWhiteSpace(change.field))
                        continue;

                    switch (change.field.ToLower())
                    {
                        case NotificationTypeModel.MessageTemplateStatusUpdate:
                            await _whatsAppWebhookHandler.HandleMessageTemplateStatusUpdate(change);
                            break;
                        default:
                            continue;
                    }
                }
            }

            return Ok();

        }
    }
}
