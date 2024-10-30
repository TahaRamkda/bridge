using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Events;
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
                _logger.LogInformation("Calling function GetClientInformation with clientId {clientId}", clientId);

                _logger.LogInformation("Executing function GetClientInformation Calling Integration GetClientAccessToken method with clientId {clientId}", clientId);

                requestStr = clientId;
                fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/clients/getclientinformation?client_Id={clientId}");

                var response = await _httpClient.GetAsync($"/clients/getclientinformation?client_Id={clientId}");
                responseStr = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response when executing function GetClientInformation of Integration GetClientInformation method with clientId {clientId} with url {url} and request {request} and content {content}", clientId, fullUrl, requestStr, responseStr);

                var result = JsonConvert.DeserializeObject<ApiResult>(responseStr);
                if (result != null && result.Success)
                {
                    var clientInfo = JsonConvert.DeserializeObject<ClientInformationDto>(JsonConvert.SerializeObject(result.Result));
                    if (clientInfo != null)
                        return clientInfo;
                }

                _logger.LogInformation("Received response when executing function GetClientInformation of Integration GetClientAccessToken method with clientId {clientId} with url {url} and request {request} and content {content}", clientId, fullUrl, requestStr, responseStr);

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
                _logger.LogInformation("Calling function GetSenderInformation with clientId {clientId} and senderNameId {senderNameId}", clientId, senderNameId);

                _logger.LogInformation("Executing function GetSenderInformation Calling Integration GetClientAccessToken method with clientId {clientId} and senderNameId {senderNameId}", clientId, senderNameId);

                requestStr = clientId;
                fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/sendernames/getsendernameinformation?client_Id={clientId}&sender_Name_Id={senderNameId}");

                var response = await _httpClient.GetAsync($"/sendernames/getsendernameinformation?client_Id={clientId}&sender_Name_Id={senderNameId}");
                responseStr = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response when executing function GetSenderInformation of Integration GetClientInformation method with clientId {clientId} and senderNameId {senderNameId} with url {url} and request {request} and content {content}", clientId, senderNameId, fullUrl, requestStr, responseStr);

                var result = JsonConvert.DeserializeObject<ApiResult>(responseStr);
                if (result != null && result.Success)
                {
                    var senderNameInfo = JsonConvert.DeserializeObject<SenderNameInformationDto>(JsonConvert.SerializeObject(result.Result));
                    if (senderNameInfo != null)
                        return senderNameInfo;
                }

                _logger.LogInformation("Received response when executing function GetSenderInformation of Integration GetClientAccessToken method with clientId {clientId} and senderNameId {senderNameId} with url {url} and request {request} and content {content}", clientId, senderNameId, fullUrl, requestStr, responseStr);

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
                _logger.LogInformation("Calling function SendMessageStatusUpdate with received object {object}", JsonConvert.SerializeObject(updateDto));

                string requestStr = String.Empty;
                string fullUrl = String.Empty;
                string responseStr = String.Empty;

                try
                {
                    var result = new ApiResult
                    {
                        StatusCode = 200,
                        Success = true,
                        Result = updateDto
                    };

                    requestStr = JsonConvert.SerializeObject(result);
                    fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/message/whatsappmessagestatusupdate");

                    _logger.LogInformation("Executing function SendMessageStatusUpdate Calling Integration whatsappmessagestatusupdate method with clientId {clientId} with url {url} and request {request}", updateDto.client_Id, fullUrl, requestStr);

                    var response = await _httpClient.PostAsync($"/message/whatsappmessagestatusupdate", new StringContent(requestStr, null, "application/json"));
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
                _logger.LogInformation("Calling function SendMessageTemplateStatusUpdate with received object {object} and sendRequestToIntegration {sendRequestToIntegration}", JsonConvert.SerializeObject(messageTemplate), sendRequestToIntegration);

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

                foreach (var component in messageTemplate.components)
                {
                    if (component.type == TemplateComponentTypeModel.HEADER)
                    {
                        templateDto.Header = new MessageTemplateDto.HeaderComponent
                        {
                            Format = component.format
                        };

                        if (templateDto.Header.Format == TemplateHeaderFormatTypeModel.TEXT)
                        {
                            templateDto.Header.Text = component.text;

                            if (component.example != null && component.example.header_text != null)
                            {
                                List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(component.example.header_text));
                                templateDto.Header.TextCount = examples.Count;

                                for (int i = 0; i < examples.Count; i++)
                                {
                                    templateDto.Header.Values.Add(new MessageTemplateDto.KeyValue
                                    {
                                        index = i + 1,
                                        value = examples[i]
                                    });
                                }
                            }
                        }
                        else if (templateDto.Header.Format == TemplateHeaderFormatTypeModel.IMAGE)
                        {
                            templateDto.Header.Text = component.text;

                            if (component.example != null && component.example.header_handle != null)
                            {
                                List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(component.example.header_handle));
                                for (int i = 0; i < examples.Count; i++)
                                {
                                    templateDto.Header.Values.Add(new MessageTemplateDto.KeyValue
                                    {
                                        index = i + 1,
                                        value = examples[i]
                                    });
                                }
                            }
                        }
                        else if (templateDto.Header.Format == TemplateHeaderFormatTypeModel.DOCUMENT)
                        {
                            templateDto.Header.Text = component.text;

                            if (component.example != null && component.example.header_handle != null)
                            {
                                List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(component.example.header_handle));
                                for (int i = 0; i < examples.Count; i++)
                                {
                                    templateDto.Header.Values.Add(new MessageTemplateDto.KeyValue
                                    {
                                        index = i + 1,
                                        value = examples[i]
                                    });
                                }
                            }
                        }
                    }
                    else if (component.type == TemplateComponentTypeModel.BODY)
                    {
                        templateDto.Body = new MessageTemplateDto.BodyComponent
                        {
                            Text = component.text
                        };

                        if (component.example != null && component.example.body_text != null)
                        {
                            var body_text = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(component.example.body_text));

                            List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(body_text[0]));
                            for (int i = 0; i < examples.Count; i++)
                            {
                                templateDto.Body.Values.Add(new MessageTemplateDto.KeyValue
                                {
                                    index = i + 1,
                                    value = examples[i]
                                });
                            }

                            templateDto.Body.TextCount = examples.Count;
                        }
                    }
                    else if (component.type == TemplateComponentTypeModel.FOOTER)
                    {
                        templateDto.Footer = new MessageTemplateDto.FooterComponent
                        {
                            Text = component.text
                        };
                    }
                    else if (component.type == TemplateComponentTypeModel.BUTTONS)
                    {
                        int buttonIndex = 1;
                        foreach (var button in component.buttons)
                        {
                            if (button.type == TemplateButtonTypeModel.QUICK_REPLY)
                            {
                                buttonIndex = 1;
                                templateDto.Buttons.Add(new MessageTemplateDto.ButtonComponent
                                {
                                    Type = button.type,
                                    Text = button.text,
                                    Index = buttonIndex
                                });

                                buttonIndex++;
                            }
                            else if (button.type == TemplateButtonTypeModel.URL)
                            {
                                var buttonComp = new MessageTemplateDto.ButtonComponent
                                {
                                    Type = button.type,
                                    Text = button.text,
                                    Url = button.url,
                                    Index = buttonIndex++
                                };

                                if (button.example != null)
                                {
                                    List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(button.example));
                                    for (int i = 0; i < examples.Count; i++)
                                    {
                                        buttonComp.Values.Add(new MessageTemplateDto.KeyValue
                                        {
                                            index = i + 1,
                                            value = examples[i]
                                        });
                                    }

                                    buttonComp.TextCount = examples.Count;
                                }

                                templateDto.Buttons.Add(buttonComp);
                            }
                            else if (button.type == TemplateButtonTypeModel.PHONE_NUMBER)
                            {
                                var buttonComp = new MessageTemplateDto.ButtonComponent
                                {
                                    Type = button.type,
                                    Text = button.text,
                                    PhoneNumber = button.phone_number,
                                    Index = buttonIndex++
                                };

                                templateDto.Buttons.Add(buttonComp);
                            }
                        }
                    }
                }

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
                        fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/templates/templatesync");

                        _logger.LogInformation("Executing function SendMessageTemplateStatusUpdate Calling Integration TemplatePost method with templateId {templateId } with url {url} and request {request}", templateDto.Id, fullUrl, requestStr);

                        var response = await _httpClient.PostAsync($"/templates/templatesync", new StringContent(requestStr, null, "application/json"));
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

        #endregion
    }
}
