using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Controllers;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
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

        public async Task HandleMessageTemplateStatusUpdate(string clientId, Change change)
        {
            try
            {
                _logger.LogInformation("Calling function HandleMessageTemplateStatusUpdate with received object {object}", JsonConvert.SerializeObject(change));

                TemplateUpdateWebhookModel templateUpdate = JsonConvert.DeserializeObject<TemplateUpdateWebhookModel>(JsonConvert.SerializeObject(change.value));

                var fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{templateUpdate.message_template_id}");
                _logger.LogInformation("Calling WhatsApp GetTemplateById method with templateId {templateId} with url {url}", templateUpdate.message_template_id, fullUrl);

                var accessToken = await _integrationHandler.GetAccessTokenByClientId(clientId);
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                var response = await _httpClient.GetAsync($"/{templateUpdate.message_template_id}");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response of WhatsApp GetTemplateById method with templateId {templateId} with url {url} and content {content}", templateUpdate.message_template_id, fullUrl, content);

                var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

                await _integrationHandler.SendMessageTemplateStatusUpdate(messageTemplate, true);

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleMessageTemplateStatusUpdate with received object {object}", ex, JsonConvert.SerializeObject(change));
            }
        }

        public async Task HandleMessageStatusUpdate(string clientId, Change change)
        {
            try
            {
                _logger.LogInformation("Calling function HandleMessageStatusUpdate with received clientId {clientId} and object {object}", clientId, JsonConvert.SerializeObject(change));

                MessageUpdateWebhookModel messageUpdate = JsonConvert.DeserializeObject<MessageUpdateWebhookModel>(JsonConvert.SerializeObject(change.value));
                if (messageUpdate.statuses != null && messageUpdate.statuses.Any())
                {
                    foreach (var status in messageUpdate.statuses)
                    {
                        var updateDto = new MessageStatusUpdateDto
                        {
                            client_Id = clientId,
                            wam_Id = status.id,
                            status = status.status,
                            update_DateTime = CommonHelper.ConvertDateTimeFormat(CommonHelper.ConvertFromEpoch(status.timestamp)),
                            recipient_Id = status.recipient_id
                        };

                        if (status.conversation != null)
                        {
                            updateDto.conversation = new MessageStatusUpdateDto.Conversation
                            {
                                id = status.conversation.id,
                                origin_type = status.conversation?.origin?.type
                            };
                        }

                        if (status.pricing != null)
                        {
                            updateDto.pricing = new MessageStatusUpdateDto.Pricing
                            {
                                billable = status.pricing.billable,
                                category = status.pricing.category,
                                pricing_model = status.pricing.pricing_model
                            };
                        }

                        if (status.errors != null && status.errors.Any())
                        {
                            var err = status.errors[0];
                            updateDto.error = new MessageStatusUpdateDto.Error
                            {
                                code = err.code,
                                title = err.title,
                                message = err.message,
                                error_Details = err.error_data?.details
                            };
                        }

                        await _integrationHandler.SendMessageStatusUpdate(updateDto);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleMessageStatusUpdate with received clientId {clientId} and object {object}", ex, clientId, JsonConvert.SerializeObject(change));
            }
        }
    }
}
