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

            if (String.IsNullOrWhiteSpace(messageTemplateId))
            {
                return Ok(new ApiResult
                {
                    StatusCode = 400,
                    Message = "Please enter message template id"
                });
            }

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

            if (String.IsNullOrWhiteSpace(messageTemplateId))
            {
                return Ok(new ApiResult
                {
                    StatusCode = 400,
                    Message = "Please enter message template id"
                });
            }

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

        [HttpPost("SendBatchTemplateMessage")]
        public async Task<IActionResult> SendBatchTemplateMessage(SendMessageTemplateRequestDto model)
        {
            _logger.LogInformation("Received SendBatchTemplateMessage request with data={data}", JsonConvert.SerializeObject(model));

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

            if (String.IsNullOrWhiteSpace(model.LanguageCode))
            {
                return BadRequest(new ApiResult
                {
                    Message = "Language code shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.TemplateId))
            {
                return BadRequest(new ApiResult
                {
                    Message = "Template id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.TemplateName))
            {
                return BadRequest(new ApiResult
                {
                    Message = "Template name shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _whatsAppHandler.HandleSendBatchTemplateMessage(model);
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
                Message = "Template sent successfully",
                Result = response
            });
        }
    }
}
