using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Models.WhatsApp.Types;
using WhatsAppBridge.Services;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Handler
{
    public partial class IntegrationHandler
    {
        #region Fields

        private readonly ILogger<IntegrationHandler> _logger;
        private readonly IOptions<IntegrationConfigurationSettings> _integrationConfigurationSettings;
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        #endregion

        #region Ctor

        public IntegrationHandler(ILogger<IntegrationHandler> logger,
          IOptions<IntegrationConfigurationSettings> integrationConfigurationSettings,
          IHttpClientFactory httpClientFactory,
          ICacheService cacheService)
        {
            _logger = logger;
            _integrationConfigurationSettings = integrationConfigurationSettings;
            _httpClient = httpClientFactory.CreateClient(HttpClientType.integration_api);
            _cacheService = cacheService;
        }

        #endregion

        #region Methods

        public async Task<ClientInformationDto> GetClientInformation(string clientId)
        {
            string requestStr = String.Empty;
            string endpoint = String.Empty;
            string responseStr = String.Empty;
            string cacheKey = String.Format(CacheKeys.CLIENTS_BY_ID_KEY, clientId);

            try
            {
                _logger.LogDebug("Calling function GetClientInformation with clientId={clientId}", clientId);

                var cachedResult = await _cacheService.GetAsync(cacheKey, async () =>
                {
                    _logger.LogDebug("Executing function GetClientInformation Calling Integration GetClientAccessToken method with clientId={clientId}", clientId);

                    requestStr = clientId;
                    endpoint = $"/clients/getclientinformation?clientId={clientId}";

                    var apiCallStart = DateTime.UtcNow;
                    var response = await _httpClient.GetAsync(endpoint);
                    responseStr = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received response when executing function GetClientInformation of Integration GetClientInformation method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request} and content={content} with apiResponseTime={apiResponseTime}", clientId, endpoint, requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var result = JsonConvert.DeserializeObject<ApiResult>(responseStr);
                    if (result != null && result.Success)
                    {
                        var clientInfo = JsonConvert.DeserializeObject<ClientInformationDto>(JsonConvert.SerializeObject(result.Result));
                        if (clientInfo != null)
                            return clientInfo;
                    }

                    return null;
                });

                //If result is null due to some reason, empty cache immediately
                if (cachedResult == null)
                    await _cacheService.RemoveAsync(cacheKey);

                return cachedResult;
            }
            catch (Exception ex)
            {
                //If result is null due to some reason, empty cache immediately
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogError("Exception occurred {exception} when executing function GetClientInformation of Integration GetClientAccessToken method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request} and content={content}", ex, clientId, endpoint, requestStr, responseStr);
            }

            return null;
        }

        public async Task<SenderNameInformationDto> GetSenderInformation(string clientId, string senderNameId)
        {
            string requestStr = String.Empty;
            string endpoint = String.Empty;
            string responseStr = String.Empty;
            string cacheKey = String.Format(CacheKeys.SENDERS_BY_CLIENTID_SENDERID_KEY, clientId, senderNameId);

            try
            {
                _logger.LogDebug("Calling function GetSenderInformation with clientId={clientId} and senderNameId={senderNameId}", clientId, senderNameId);

                var cachedResult = await _cacheService.GetAsync(cacheKey, async () =>
                {
                    _logger.LogDebug("Executing function GetSenderInformation Calling Integration GetClientAccessToken method with clientId={clientId} and senderNameId={senderNameId}", clientId, senderNameId);

                    requestStr = clientId;
                    endpoint = $"/sendernames/getsendernameinformation?clientId={clientId}&senderNameId={senderNameId}";

                    var apiCallStart = DateTime.UtcNow;
                    var response = await _httpClient.GetAsync(endpoint);
                    responseStr = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received response when executing function GetSenderInformation of Integration GetClientInformation method with clientId={clientId} and senderNameId={senderNameId} with apiEndpoint={apiEndpoint} and request={request} and content={content} and apiResponseTime={apiResponseTime}", clientId, senderNameId, endpoint, requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var result = JsonConvert.DeserializeObject<ApiResult>(responseStr);
                    if (result != null && result.Success)
                    {
                        var senderNameInfo = JsonConvert.DeserializeObject<SenderNameInformationDto>(JsonConvert.SerializeObject(result.Result));
                        if (senderNameInfo != null)
                            return senderNameInfo;
                    }

                    return null;
                });

                //If result is null due to some reason, empty cache immediately
                if (cachedResult == null)
                    await _cacheService.RemoveAsync(cacheKey);

                return cachedResult;
            }
            catch (Exception ex)
            {
                //If result is null due to some reason, empty cache immediately
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogError("Exception occurred {exception} when executing function GetSenderInformation of Integration GetClientAccessToken method with clientId={clientId} and senderNameId={senderNameId} with apiEndpoint={apiEndpoint} and request={request} and content={content}", ex, clientId, senderNameId, endpoint, requestStr, responseStr);
            }

            return null;
        }

        public async Task SendMessageStatusUpdate(MessageStatusUpdateDto updateDto)
        {
            try
            {
                _logger.LogDebug("Calling function SendMessageStatusUpdate with received object={object}", JsonConvert.SerializeObject(updateDto));

                string requestStr = String.Empty;
                string endpoint = String.Empty;
                string responseStr = String.Empty;

                try
                {
                    //This is to avoid the deadlocks in database when facebook is sending message status update very frequently for same WA ID
                    if (_integrationConfigurationSettings.Value.DelaySendingStatusUpdate)
                    {
                        Random random = new Random();
                        int delayMilliseconds = random.Next(1000, 5001); // Random delay between 1000ms (1s) and 5000ms (5s)
                        await Task.Delay(delayMilliseconds); // Asynchronous delay
                    }

                    requestStr = JsonConvert.SerializeObject(updateDto);
                    endpoint = $"/bridge/whatsappmessagestatusupdate";

                    _logger.LogDebug("Executing function SendMessageStatusUpdate Calling Integration whatsappmessagestatusupdate method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request}", updateDto.client_Id, endpoint, requestStr);

                    var apiCallStart = DateTime.UtcNow;
                    var response = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));
                    if (!response.IsSuccessStatusCode)
                        responseStr = String.Concat("Status code: ", response.StatusCode, " | Reason: ", response.ReasonPhrase);
                    else
                        responseStr = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received response when executing function SendMessageStatusUpdate of Integration whatsappmessagestatusupdate method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request} and content={content} with apiResponseTime={apiResponseTime}", updateDto.client_Id, endpoint, requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occurred {exception} when executing function SendMessageStatusUpdate of Integration whatsappmessagestatusupdate method with clientId={clientId} with url={url} and request={request} and content {content}", ex, updateDto.client_Id, endpoint, requestStr, responseStr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function SendMessageStatusUpdate with received object={object}", ex, JsonConvert.SerializeObject(updateDto));
            }
        }

        public async Task<MessageTemplateDto> SendMessageTemplateStatusUpdate(MessageTemplateModel messageTemplate, bool sendRequestToIntegration)
        {
            try
            {
                _logger.LogDebug("Calling function SendMessageTemplateStatusUpdate with received object={object} and sendRequestToIntegration={sendRequestToIntegration}", JsonConvert.SerializeObject(messageTemplate), sendRequestToIntegration);

                MessageTemplateDto templateDto = new MessageTemplateDto
                {
                    Id = messageTemplate.id,
                    Name = messageTemplate.name,
                    Language = messageTemplate.language,
                    Status = messageTemplate.status,
                    IsApproved = messageTemplate.status == TemplateStatusModel.APPROVED.ToString() ? true : false,
                    //Category = messageTemplate.category,
                    //SubCategory = messageTemplate.sub_category
                };

                if (sendRequestToIntegration)
                {
                    string requestStr = String.Empty;
                    string endpoint = String.Empty;
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
                        endpoint = $"/bridge/templatesync";

                        _logger.LogDebug("Executing function SendMessageTemplateStatusUpdate Calling Integration TemplatePost method with templateId={templateId} with apiEndpoint={apiEndpoint} and request={request}", templateDto.Id, endpoint, requestStr);

                        var apiCallStart = DateTime.UtcNow;
                        var response = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));
                        responseStr = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received response when executing function SendMessageTemplateStatusUpdate of Integration TemplatePost method with templateId={templateId} with apiEndpoint={apiEndpoint} and request={request} and content={content} with apiResponseTime={apiResponseTime}", templateDto.Id, endpoint, requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occurred {exception} when executing function SendMessageTemplateStatusUpdate of Integration TemplatePost method with templateId={templateId} with apiEndpoint={apiEndpoint} and request={request} and content={content}", ex, templateDto.Id, endpoint, requestStr, responseStr);
                    }
                }

                return templateDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function SendMessageTemplateStatusUpdate with received object={object}", ex, JsonConvert.SerializeObject(messageTemplate));
                return null;
            }
        }

        public async Task MessageReceiveUpdate(MessageReceiveDto updateDto)
        {
            try
            {
                _logger.LogDebug("Calling function MessageReceiveUpdate with received object={object}", JsonConvert.SerializeObject(updateDto));

                string requestStr = String.Empty;
                string endpoint = String.Empty;
                string responseStr = String.Empty;

                try
                {
                    var result = updateDto;

                    requestStr = JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    endpoint = $"/bridge/whatsappmessagereceive";
                    if (updateDto.flowResponse != null)
                        endpoint = $"/bridge/flowresponse";
                    if (updateDto.order != null)
                        endpoint = $"/bridge/order";
                    
                    _logger.LogDebug("Executing function MessageReceiveUpdate Calling Integration whatsappmessagereceive method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request}", updateDto.client_Id, endpoint, requestStr);

                    var apiCallStart = DateTime.UtcNow;
                    var response = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));

                    if (!response.IsSuccessStatusCode)
                        responseStr = String.Concat("Status code: ", response.StatusCode, " | Reason: ", response.ReasonPhrase);
                    else
                        responseStr = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received response when executing function MessageReceiveUpdate of Integration whatsappmessagereceive method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request} and content={content} with apiResponseTime={apiResponseTime}", updateDto.client_Id, endpoint, requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occurred {exception} when executing function MessageReceiveUpdate of Integration whatsappmessagereceive method with clientId={clientId} with apiEndpoint={apiEndpoint} and request={request} and content={content}", ex, updateDto.client_Id, endpoint, requestStr, responseStr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function MessageReceiveUpdate with received object={object}", ex, JsonConvert.SerializeObject(updateDto));
            }
        }

        #endregion
    }
}
