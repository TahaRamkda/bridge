using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Models.WhatsApp.Types;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Handler
{
    public partial class IntegrationHandler
    {
        #region Fields

        private readonly ILogger<IntegrationHandler> _logger;
        private readonly IOptions<IntegrationConfigurationSettings> _integrationConfigurationSettings;
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = String.Empty;

        #endregion

        #region Ctor

        public IntegrationHandler(ILogger<IntegrationHandler> logger,
          IOptions<IntegrationConfigurationSettings> integrationConfigurationSettings,
          IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _integrationConfigurationSettings = integrationConfigurationSettings;
            _httpClient = httpClientFactory.CreateClient(HttpClientType.integration_api);
            baseUrl = _httpClient.BaseAddress.AbsoluteUri;
        }

        #endregion

        #region Methods

        public async Task<ClientInformationDto> GetClientInformation(string clientId)
        {
            string requestStr = String.Empty;
            string fullUrl = String.Empty;
            string responseStr = String.Empty;

            try
            {
                _logger.LogDebug("Calling function GetClientInformation with clientId {clientId}", clientId);

                _logger.LogDebug("Executing function GetClientInformation Calling Integration GetClientAccessToken method with clientId {clientId}", clientId);

                requestStr = clientId;
                fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/clients/getclientinformation?clientId={clientId}");

                var response = await _httpClient.GetAsync($"/clients/getclientinformation?clientId={clientId}");
                responseStr = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("Received response when executing function GetClientInformation of Integration GetClientInformation method with clientId {clientId} with url {url} and request {request} and content {content}", clientId, fullUrl, requestStr, responseStr);

                var result = JsonConvert.DeserializeObject<ApiResult>(responseStr);
                if (result != null && result.Success)
                {
                    var clientInfo = JsonConvert.DeserializeObject<ClientInformationDto>(JsonConvert.SerializeObject(result.Result));
                    if (clientInfo != null)
                        return clientInfo;
                }

                _logger.LogDebug("Received response when executing function GetClientInformation of Integration GetClientAccessToken method with clientId {clientId} with url {url} and request {request} and content {content}", clientId, fullUrl, requestStr, responseStr);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function GetClientInformation of Integration GetClientAccessToken method with clientId {clientId} with url {url} and request {request} and content {content}", ex, clientId, fullUrl, requestStr, responseStr);
            }

            return null;
        }

        public async Task<SenderNameInformationDto> GetSenderInformation(string clientId, string senderNameId)
        {
            string requestStr = String.Empty;
            string fullUrl = String.Empty;
            string responseStr = String.Empty;

            try
            {
                _logger.LogDebug("Calling function GetSenderInformation with clientId {clientId} and senderNameId {senderNameId}", clientId, senderNameId);

                _logger.LogDebug("Executing function GetSenderInformation Calling Integration GetClientAccessToken method with clientId {clientId} and senderNameId {senderNameId}", clientId, senderNameId);

                requestStr = clientId;
                fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/sendernames/getsendernameinformation?clientId={clientId}&senderNameId={senderNameId}");

                var response = await _httpClient.GetAsync($"/sendernames/getsendernameinformation?clientId={clientId}&senderNameId={senderNameId}");
                responseStr = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("Received response when executing function GetSenderInformation of Integration GetClientInformation method with clientId {clientId} and senderNameId {senderNameId} with url {url} and request {request} and content {content}", clientId, senderNameId, fullUrl, requestStr, responseStr);

                var result = JsonConvert.DeserializeObject<ApiResult>(responseStr);
                if (result != null && result.Success)
                {
                    var senderNameInfo = JsonConvert.DeserializeObject<SenderNameInformationDto>(JsonConvert.SerializeObject(result.Result));
                    if (senderNameInfo != null)
                        return senderNameInfo;
                }

                _logger.LogDebug("Received response when executing function GetSenderInformation of Integration GetClientAccessToken method with clientId {clientId} and senderNameId {senderNameId} with url {url} and request {request} and content {content}", clientId, senderNameId, fullUrl, requestStr, responseStr);

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function GetSenderInformation of Integration GetClientAccessToken method with clientId {clientId} and senderNameId {senderNameId} with url {url} and request {request} and content {content}", ex, clientId, senderNameId, fullUrl, requestStr, responseStr);
            }

            return null;
        }

        public async Task SendMessageStatusUpdate(MessageStatusUpdateDto updateDto)
        {
            try
            {
                _logger.LogDebug("Calling function SendMessageStatusUpdate with received object {object}", JsonConvert.SerializeObject(updateDto));

                string requestStr = String.Empty;
                string fullUrl = String.Empty;
                string responseStr = String.Empty;

                try
                {
                    requestStr = JsonConvert.SerializeObject(updateDto);
                    fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/bridge/whatsappmessagestatusupdate");

                    _logger.LogDebug("Executing function SendMessageStatusUpdate Calling Integration whatsappmessagestatusupdate method with clientId {clientId} with url {url} and request {request}", updateDto.client_Id, fullUrl, requestStr);

                    var response = await _httpClient.PostAsync($"/bridge/whatsappmessagestatusupdate", new StringContent(requestStr, null, "application/json"));
                    if (!response.IsSuccessStatusCode)
                        responseStr = String.Concat("Status code: ", response.StatusCode, " | Reason: ", response.ReasonPhrase);
                    else
                        responseStr = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received response when executing function SendMessageStatusUpdate of Integration whatsappmessagestatusupdate method with clientId {clientId} with url {url} and request {request} and content {content}", updateDto.client_Id, fullUrl, requestStr, responseStr);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occurred {exception} when executing function SendMessageStatusUpdate of Integration whatsappmessagestatusupdate method with clientId {clientId} with url {url} and request {request} and content {content}", ex, updateDto.client_Id, fullUrl, requestStr, responseStr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function SendMessageStatusUpdate with received object {object}", ex, JsonConvert.SerializeObject(updateDto));
            }
        }

        public async Task<MessageTemplateDto> SendMessageTemplateStatusUpdate(MessageTemplateModel messageTemplate, bool sendRequestToIntegration)
        {
            try
            {
                _logger.LogDebug("Calling function SendMessageTemplateStatusUpdate with received object {object} and sendRequestToIntegration {sendRequestToIntegration}", JsonConvert.SerializeObject(messageTemplate), sendRequestToIntegration);

                MessageTemplateDto templateDto = new MessageTemplateDto
                {
                    Id = messageTemplate.id,
                    Name = messageTemplate.name,
                    Language = messageTemplate.language,
                    Status = messageTemplate.status,
                    IsApproved = messageTemplate.status == TemplateStatusModel.APPROVED.ToString() ? true : false,
                    Category = messageTemplate.category,
                    SubCategory = messageTemplate.sub_category
                };

                if (sendRequestToIntegration)
                {
                    string requestStr = String.Empty;
                    string fullUrl = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var result = new ApiResult
                        {
                            StatusCode = 200,
                            Success = true,
                            Result = templateDto
                        };

                        requestStr = JsonConvert.SerializeObject(result);
                        fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/bridge/templatesync");

                        _logger.LogDebug("Executing function SendMessageTemplateStatusUpdate Calling Integration TemplatePost method with templateId {templateId } with url {url} and request {request}", templateDto.Id, fullUrl, requestStr);

                        var response = await _httpClient.PostAsync($"/bridge/templatesync", new StringContent(requestStr, null, "application/json"));
                        responseStr = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received response when executing function SendMessageTemplateStatusUpdate of Integration TemplatePost method with templateId {templateId } with url {url} and request {request} and content {content}", templateDto.Id, fullUrl, requestStr, responseStr);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occurred {exception} when executing function SendMessageTemplateStatusUpdate of Integration TemplatePost method with templateId {templateId } with url {url} and request {request} and content {content}", ex, templateDto.Id, fullUrl, requestStr, responseStr);
                    }
                }

                return templateDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function SendMessageTemplateStatusUpdate with received object {object}", ex, JsonConvert.SerializeObject(messageTemplate));
                return null;
            }
        }

        public async Task MessageReceiveUpdate(MessageReceiveDto updateDto)
        {
            try
            {
                _logger.LogDebug("Calling function MessageReceiveUpdate with received object {object}", JsonConvert.SerializeObject(updateDto));

                string requestStr = String.Empty;
                string fullUrl = String.Empty;
                string responseStr = String.Empty;

                try
                {
                    var result = updateDto;

                    requestStr = JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    string endpoint = $"/bridge/whatsappmessagereceive";
                    if (updateDto.flowResponse != null)
                        endpoint = $"/bridge/flowresponse";

                    fullUrl = CommonHelper.GetFullUrl(baseUrl, endpoint);

                    _logger.LogDebug("Executing function MessageReceiveUpdate Calling Integration whatsappmessagereceive method with clientId {clientId} with url {url} and request {request}", updateDto.client_Id, fullUrl, requestStr);

                    var response = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));

                    if (!response.IsSuccessStatusCode)
                        responseStr = String.Concat("Status code: ", response.StatusCode, " | Reason: ", response.ReasonPhrase);
                    else
                        responseStr = await response.Content.ReadAsStringAsync();

                    _logger.LogDebug("Received response when executing function MessageReceiveUpdate of Integration whatsappmessagereceive method with clientId {clientId} with url {url} and request {request} and content {content}", updateDto.client_Id, fullUrl, requestStr, responseStr);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occurred {exception} when executing function MessageReceiveUpdate of Integration whatsappmessagereceive method with clientId {clientId} with url {url} and request {request} and content {content}", ex, updateDto.client_Id, fullUrl, requestStr, responseStr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function MessageReceiveUpdate with received object {object}", ex, JsonConvert.SerializeObject(updateDto));
            }
        }

        #endregion
    }
}
