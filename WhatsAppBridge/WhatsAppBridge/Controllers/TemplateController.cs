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
        public async Task<IActionResult> GetTemplatebyId(string clientId, string messageTemplateId)
        {
            _logger.LogInformation("Calling api GetTemplatebyId with templateId={templateId}", messageTemplateId);

            if (String.IsNullOrWhiteSpace(clientId))
            {
                return Ok(new ApiResult
                {
                    Message = "Client id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(messageTemplateId))
            {
                return Ok(new ApiResult
                {
                    StatusCode = 400,
                    Message = "Message template id shouldn't be empty"
                });
            }

            var fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{messageTemplateId}");
            _logger.LogDebug("Calling WhatsApp GetTemplateById method with templateId={templateId} with url={url}", messageTemplateId, fullUrl);

            var clientInfo = await _integrationHandler.GetClientInformation(clientId);
            if (clientInfo == null)
            {
                return Ok(new ApiResult
                {
                    StatusCode = 404,
                    Message = $"Client not found with clientId: {clientId}"
                });
            }

            var apiCallStart = DateTime.UtcNow;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {clientInfo.AccessToken}");
            var response = await _httpClient.GetAsync($"/{messageTemplateId}");
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Received response of WhatsApp GetTemplateById method with templateId={templateId} with apiEndpoint={apiEndpoint} and content={content} with apiResponseTime={apiResponseTime}", messageTemplateId, fullUrl, content, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

            var templateDto = await _integrationHandler.SendMessageTemplateStatusUpdate(messageTemplate, false);

            return Ok(new ApiResult
            {
                Success = true,
                StatusCode = 200,
                Result = templateDto
            });
        }

        [HttpGet("SyncTemplatebyId")]
        public async Task<IActionResult> SyncTemplatebyId(string clientId, string messageTemplateId)
        {
            _logger.LogInformation("Calling api SyncTemplatebyId with templateId={templateId}", messageTemplateId);

            if (String.IsNullOrWhiteSpace(clientId))
            {
                return Ok(new ApiResult
                {
                    Message = "Client id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(messageTemplateId))
            {
                return Ok(new ApiResult
                {
                    StatusCode = 400,
                    Message = "Message template id shouldn't be empty"
                });
            }

            var fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{messageTemplateId}");

            _logger.LogDebug("Calling WhatsApp SyncTemplatebyId method with templateId={templateId} with url={url}", messageTemplateId, fullUrl);

            var clientInfo = await _integrationHandler.GetClientInformation(clientId);
            if (clientInfo == null)
            {
                return Ok(new ApiResult
                {
                    StatusCode = 404,
                    Message = $"Client not found with clientId: {clientId}"
                });
            }

            var apiCallStart = DateTime.UtcNow;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {clientInfo.AccessToken}");
            var response = await _httpClient.GetAsync($"/{messageTemplateId}");
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Received response of WhatsApp SyncTemplatebyId method with templateId={templateId} with apiEndpoint={apiEndpoint} and content={content} with apiResponseTime={apiResponseTime}", messageTemplateId, fullUrl, content, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

            var templateDto = await _integrationHandler.SendMessageTemplateStatusUpdate(messageTemplate, true);

            return Ok(new ApiResult
            {
                Success = templateDto != null,
                StatusCode = 200
            });
        }

        [HttpPost("SendBatchTemplateMessage")]
        public async Task<IActionResult> SendBatchTemplateMessage(SendMessageTemplateRequestDto model)
        {
            _logger.LogInformation("Received SendBatchTemplateMessage request with data={data}", JsonConvert.SerializeObject(model));

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
                    Message = "Client Id shouldn't be empty",
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

            if (String.IsNullOrWhiteSpace(model.LanguageCode))
            {
                return Ok(new ApiResult
                {
                    Message = "Language code shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.TemplateId))
            {
                return Ok(new ApiResult
                {
                    Message = "Template id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.TemplateName))
            {
                return Ok(new ApiResult
                {
                    Message = "Template name shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _whatsAppHandler.HandleSendBatchTemplateMessage(model);
            return Ok(response);
        }

        [HttpPost("TemplateMessageOps")]
        public async Task<IActionResult> TemplateMessageOps(CreateMessageTemplateRequestDto model)
        {
            _logger.LogInformation("Received CreateMessageTemplateRequestDto request with data={data}", JsonConvert.SerializeObject(model));

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
                    Message = "Client Id shouldn't be empty",
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

            if (String.IsNullOrWhiteSpace(model.LanguageCode))
            {
                return Ok(new ApiResult
                {
                    Message = "Language code shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.Name))
            {
                return Ok(new ApiResult
                {
                    Message = "Name shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.Category))
            {
                return Ok(new ApiResult
                {
                    Message = "Category shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var resp = await _whatsAppHandler.HandleMessageTemplateOps(model);

            return Ok(resp);
        }

        [HttpPost("CarouselTemplateMessageOps")]
        public async Task<IActionResult> CarouselTemplateMessageOps(CreateCarouselTemplateRequestDto model)
        {
            _logger.LogInformation("Received CarouselTemplateMessageOps request with data={data}", JsonConvert.SerializeObject(model));

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
                    Message = "Client Id shouldn't be empty",
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

            if (String.IsNullOrWhiteSpace(model.LanguageCode))
            {
                return Ok(new ApiResult
                {
                    Message = "Language code shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.Name))
            {
                return Ok(new ApiResult
                {
                    Message = "Name shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.Category))
            {
                return Ok(new ApiResult
                {
                    Message = "Category shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (model.Body == null)
            {
                return Ok(new ApiResult
                {
                    Message = "Main body is required in carousel",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (!model.Cards.Any())
            {
                return Ok(new ApiResult
                {
                    Message = "Cards shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (model.Cards.Any(x => x.Body != null && !String.IsNullOrWhiteSpace(x.Body.Text) && x.Body.Text.Length > 150))
            {
                return Ok(new ApiResult
                {
                    Message = "Cards body text shouldn't be greater than 150 characters",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var resp = await _whatsAppHandler.HandleCarouselTemplateOps(model);

            return Ok(resp);
        }

        [HttpPost("SendBatchCarouselMessage")]
        public async Task<IActionResult> SendBatchCarouselMessage(SendMessageCarouselRequestDto model)
        {
            _logger.LogInformation("Received SendBatchCarouselMessage request with data={data}", JsonConvert.SerializeObject(model));

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
                    Message = "Client Id shouldn't be empty",
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

            if (String.IsNullOrWhiteSpace(model.LanguageCode))
            {
                return Ok(new ApiResult
                {
                    Message = "Language code shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.TemplateId))
            {
                return Ok(new ApiResult
                {
                    Message = "Template id shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (String.IsNullOrWhiteSpace(model.TemplateName))
            {
                return Ok(new ApiResult
                {
                    Message = "Template name shouldn't be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var response = await _whatsAppHandler.HandleSendBatchCarouselMessage(model);
            return Ok(response);
        }

    }
}
