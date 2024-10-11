using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Controllers;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Models.WhatsApp.Webhook;
using WhatsAppBridge.Settings;
using static WhatsAppBridge.Models.WhatsApp.Webhook.WhatsAppWebhookModel;

namespace WhatsAppBridge.Handler
{
    public partial class WhatsAppWebhookHandler
    {
        private readonly ILogger<WhatsAppWebhookHandler> _logger;
        private readonly IOptions<WhatsAppConfigurationSetting> _whatsAppConfigurationSetting;
        private readonly IntegrationHandler _integrationHandler;
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = String.Empty;

        public WhatsAppWebhookHandler(ILogger<WhatsAppWebhookHandler> logger,
            IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
            IntegrationHandler integrationHandler,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _whatsAppConfigurationSetting = whatsAppConfigurationSetting;
            _integrationHandler = integrationHandler;
            _httpClient = httpClientFactory.CreateClient(HttpClientType.facebook_graph_api);
            baseUrl = _httpClient.BaseAddress.AbsoluteUri;
        }

        public async Task HandleMessageTemplateStatusUpdate(Change change)
        {
            try
            {
                _logger.LogInformation("Calling function HandleMessageTemplateStatusUpdate with received object {object}", JsonConvert.SerializeObject(change));

                TemplateUpdateWebhook templateUpdate = JsonConvert.DeserializeObject<TemplateUpdateWebhook>(JsonConvert.SerializeObject(change.value));

                var fullUrl = CommonFunction.GetFullUrl(baseUrl, $"/{templateUpdate.message_template_id}");
                _logger.LogInformation("Calling WhatsApp GetTemplateById method with templateId {templateId } with url {url}", templateUpdate.message_template_id, fullUrl);

                var response = await _httpClient.GetAsync($"/{templateUpdate.message_template_id}");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response of WhatsApp GetTemplateById method with templateId {templateId } with url {url} and content {content}", templateUpdate.message_template_id, fullUrl, content);

                var messageTemplate = JsonConvert.DeserializeObject<MessageTemplate>(await response.Content.ReadAsStringAsync());

                await _integrationHandler.SendMessageTemplateStatusUpdate(messageTemplate);

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleMessageTemplateStatusUpdate with received object {object}", ex, JsonConvert.SerializeObject(change));
            }
        }
    }
}
