using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Crypto.Flow;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Models;
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
        private readonly IntegrationHandler _integrationHandler;

        public WebhookController(ILogger<WebhookController> logger,
            IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
            WhatsAppWebhookHandler whatsAppWebhookHandler,
            IntegrationHandler integrationHandler)
        {
            _logger = logger;
            _whatsAppConfigurationSetting = whatsAppConfigurationSetting;
            _whatsAppWebhookHandler = whatsAppWebhookHandler;
            _integrationHandler = integrationHandler;
        }

        [HttpGet]
        public ActionResult Get([FromQuery(Name = "hub.mode")] string hubMode = "", [FromQuery(Name = "hub.challenge")] int hubChallenge = 0, [FromQuery(Name = "hub.verify_token")] string hubVerifyToken = "")
        {
            _logger.LogDebug("Webhook received with request, hub.mode={hubmode}, hub.challenge={hubchallenge},hub.verify_token={verify_token}", hubMode, hubChallenge, hubVerifyToken);

            if (!String.IsNullOrEmpty(hubVerifyToken) && hubChallenge > 0)
            {
                if (hubVerifyToken == _whatsAppConfigurationSetting.Value.WebhookVerificationToken)
                {
                    _logger.LogDebug("Accepted verify webhook request with success with hub.mode={hubmode}, hub.challenge={hubchallenge},hub.verify_token={verify_token}", hubMode, hubChallenge, hubVerifyToken);
                    return Ok(hubChallenge);
                }
                else
                {
                    _logger.LogError("Declined verify webhook request with unauthorized with hub.mode={hubmode}, hub.challenge={hubchallenge},hub.verify_token={verify_token}", hubMode, hubChallenge, hubVerifyToken);
                    return Unauthorized("Unauthorized request");
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post(string clientId, object payload)
        {
            var data = System.Text.Json.JsonSerializer.Serialize(payload);
            _logger.LogInformation("Facebook webhook received with clientId={clientId} and data={data}", clientId, data);

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
                            await _whatsAppWebhookHandler.HandleMessageTemplateStatusUpdate(clientId, change);
                            break;
                        case NotificationTypeModel.MessageUpdate:
                            await _whatsAppWebhookHandler.HandleMessageStatusUpdate(clientId, change);
                            break;
                        default:
                            continue;
                    }
                }
            }

            return Ok();

        }

        [HttpPost("FlowHealthCheck")]
        public async Task<IActionResult> FlowHealthCheck(string clientId, string senderId, object payload)
        {
            var data = System.Text.Json.JsonSerializer.Serialize(payload);
            _logger.LogInformation("Facebook flow healthcheck webhook received with clientId={clientId} and senderId={senderId} and data={data}", clientId, senderId, data);

            var PASSPHRASE = _whatsAppConfigurationSetting.Value.WebhookVerificationToken;

            var senderInfo = await _integrationHandler.GetSenderInformation(clientId, senderId);
            if (senderInfo == null)
                return BadRequest(new ApiResult { StatusCode = 404, Message = $"Sender not found with clientId: {clientId} and senderNameId:{senderId}" });

            var PRIVATE_KEY = senderInfo.PrivateCertificate;

            var model = JsonConvert.DeserializeObject<FlowHealthCheckModel>(data);
            if (model == null)
                return BadRequest("Cannot parse received facebook flow webhook data object");

            var decrypted = EncryptionUtils.DecryptRequest(model.encrypted_aes_key, model.encrypted_flow_data, model.initial_vector, PRIVATE_KEY, PASSPHRASE);

            var response = new { data = new { status = "active" } };
            var encryptedResponse = EncryptionUtils.EncryptResponse(response, decrypted.aesKeyBytes, decrypted.initialVectorBytes);

            return Ok(encryptedResponse);
        }
    }
}
