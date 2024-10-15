using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Handler;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthenticationSchemes.ApiKeyPolicy)]
    public class TemplateController : ControllerBase
    {
        private readonly ILogger<TemplateController> _logger;
        private readonly IOptions<WhatsAppConfigurationSetting> _whatsAppConfigurationSetting;
        private readonly WhatsAppWebhookHandler _whatsAppWebhookHandler;
        private readonly IntegrationHandler _integrationHandler;
        private readonly WhatsAppHandler _whatsAppHandler;
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = String.Empty;

        public TemplateController(ILogger<TemplateController> logger,
          IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
          WhatsAppWebhookHandler whatsAppWebhookHandler,
          WhatsAppHandler whatsAppHandler,
          IntegrationHandler integrationHandler,
          IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _whatsAppConfigurationSetting = whatsAppConfigurationSetting;
            _whatsAppWebhookHandler = whatsAppWebhookHandler;
            _integrationHandler = integrationHandler;
            _whatsAppHandler = whatsAppHandler;
            _httpClient = httpClientFactory.CreateClient(HttpClientType.facebook_graph_api);
            baseUrl = _httpClient.BaseAddress.AbsoluteUri;
        }

        [HttpGet("GetTemplatebyId")]
        public async Task<IActionResult> GetTemplatebyId(string messageTemplateId)
        {
            _logger.LogInformation("Calling api GetTemplatebyId with templateId={templateId}", messageTemplateId);

            var fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{messageTemplateId}");

            _logger.LogInformation("Calling WhatsApp GetTemplateById method with templateId {templateId } with url {url}", messageTemplateId, fullUrl);

            var response = await _httpClient.GetAsync($"/{messageTemplateId}");
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Received response of WhatsApp GetTemplateById method with templateId {templateId } with url {url} and content {content}", messageTemplateId, fullUrl, content);

            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

            var templateDto = await _integrationHandler.SendMessageTemplateStatusUpdate(messageTemplate, false);

            return Ok(new ApiResult
            {
                StatusCode = 200,
                Success = true,
                Result = templateDto
            });
        }

        [HttpGet("SyncTemplatebyId")]
        public async Task<IActionResult> SyncTemplatebyId(string messageTemplateId)
        {
            _logger.LogInformation("Calling api SyncTemplatebyId with templateId={templateId}", messageTemplateId);

            var fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{messageTemplateId}");

            _logger.LogInformation("Calling WhatsApp SyncTemplatebyId method with templateId {templateId } with url {url}", messageTemplateId, fullUrl);

            var response = await _httpClient.GetAsync($"/{messageTemplateId}");
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Received response of WhatsApp SyncTemplatebyId method with templateId {templateId } with url {url} and content {content}", messageTemplateId, fullUrl, content);

            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

            var templateDto = await _integrationHandler.SendMessageTemplateStatusUpdate(messageTemplate, true);

            return Ok(new ApiResult
            {
                StatusCode = 200,
                Success = templateDto != null
            });
        }

        [HttpPost("SendTemplateMessage")]
        public async Task<IActionResult> SendTemplateMessage(SendMessageTemplateRequestDto data)
        {
            _logger.LogInformation("Received SendTemplateMessage request with data={data}", JsonConvert.SerializeObject(data));

            if (data == null)
                return BadRequest("Cannot parse received data object");

            var response = await _whatsAppHandler.HandleSendTemplateMessage(data);

            if (response == null)
            {
                return Ok(new ApiResult
                {
                    StatusCode = 400,
                    Result = "Something went wrong while sending message"
                });
            }

            return Ok(new ApiResult
            {
                Success = response.Status == 200,
                Result = response
            });
        }
    }
}
