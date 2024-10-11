using Azure.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Policy;
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
    public partial class IntegrationHandler
    {
        private readonly ILogger<IntegrationHandler> _logger;
        private readonly IOptions<IntegrationConfigurationSettings> _integrationConfigurationSettings;
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = String.Empty;

        public IntegrationHandler(ILogger<IntegrationHandler> logger,
          IOptions<IntegrationConfigurationSettings> integrationConfigurationSettings,
          IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _integrationConfigurationSettings = integrationConfigurationSettings;
            _httpClient = httpClientFactory.CreateClient(HttpClientType.integration_api);
            baseUrl = _httpClient.BaseAddress.AbsoluteUri;
        }

        public async Task SendMessageTemplateStatusUpdate(MessageTemplate messageTemplate)
        {
            try
            {
                _logger.LogInformation("Calling function SendMessageTemplateStatusUpdate with received object {object}", JsonConvert.SerializeObject(messageTemplate));

                MessageTemplateDto templateDto = new MessageTemplateDto
                {
                    Id = messageTemplate.id,
                    Name = messageTemplate.name,
                    Language = messageTemplate.language,
                    Status = messageTemplate.status,
                    IsApproved = messageTemplate.status == TemplateStatus.APPROVED.ToString() ? true : false,
                    Category = messageTemplate.category,
                    SubCategory = messageTemplate.sub_category
                };

                foreach (var component in messageTemplate.components)
                {
                    if (component.type == TemplateComponentType.HEADER)
                    {
                        templateDto.Header = new MessageTemplateDto.HeaderComponent
                        {
                            Format = component.format
                        };

                        if (templateDto.Header.Format == TemplateHeaderFormat.TEXT)
                        {
                            templateDto.Header.Text = component.text;

                            if (component.example != null && component.example.header_text != null)
                            {
                                List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(component.example.header_text));
                                templateDto.Header.TextCount = examples.Count;
                                templateDto.Header.Values = examples;
                            }
                        }
                        else if (templateDto.Header.Format == TemplateHeaderFormat.IMAGE)
                        {
                            templateDto.Header.Text = component.text;

                            if (component.example != null && component.example.handle != null)
                            {
                                List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(component.example.header_handle));
                                templateDto.Header.Values = examples;
                            }
                        }
                        else if (templateDto.Header.Format == TemplateHeaderFormat.DOCUMENT)
                        {
                            templateDto.Header.Text = component.text;

                            if(component.example.header_handle != null)
                            {
                                List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(component.example.header_handle));
                                templateDto.Header.Values = examples;
                            }
                        }
                    }
                    else if (component.type == TemplateComponentType.BODY)
                    {
                        templateDto.Body = new MessageTemplateDto.BodyComponent
                        {
                            Text = component.text
                        };

                        if (component.example != null && component.example.body_text != null)
                        {
                            var body_text = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(component.example.body_text));

                            List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(body_text[0]));
                            templateDto.Body.Values = examples;
                            templateDto.Body.TextCount = examples.Count;
                        }
                    }
                    else if (component.type == TemplateComponentType.FOOTER)
                    {
                        templateDto.Footer = new MessageTemplateDto.FooterComponent
                        {
                            Text = component.text
                        };
                    }
                    else if (component.type == TemplateComponentType.BUTTONS)
                    {
                        foreach (var button in component.buttons)
                        {
                            if (button.type == TemplateButtonType.QUICK_REPLY)
                            {
                                templateDto.Buttons.Add(new MessageTemplateDto.ButtonComponent
                                {
                                    Type = button.type,
                                    Text = button.text
                                });
                            }
                            else if (button.type == TemplateButtonType.URL)
                            {
                                var buttonComp = new MessageTemplateDto.ButtonComponent
                                {
                                    Type = button.type,
                                    Text = button.text,
                                    Url = button.url,
                                };

                                if (button.example != null)
                                {
                                    List<string> examples = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(button.example));
                                    buttonComp.Values = examples;
                                    buttonComp.TextCount = examples.Count;
                                }

                                templateDto.Buttons.Add(buttonComp);
                            }
                            else if (button.type == TemplateButtonType.PHONE_NUMBER)
                            {
                                var buttonComp = new MessageTemplateDto.ButtonComponent
                                {
                                    Type = button.type,
                                    Text = button.text,
                                    PhoneNumber = button.phone_number
                                };

                                templateDto.Buttons.Add(buttonComp);
                            }
                        }
                    }
                }

                var requestStr = JsonConvert.SerializeObject(templateDto);
                var fullUrl = CommonFunction.GetFullUrl(baseUrl, $"//templates/templatepost");

                _logger.LogInformation("Calling Integration TemplatePost method with templateId {templateId } with url {url} and request {request}", templateDto.Id, fullUrl, requestStr);

                var response = await _httpClient.PostAsync($"/templates/templatepost", new StringContent(requestStr, null, "application/json"));
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response of Integration TemplatePost method with templateId {templateId } with url {url} and request {request} and content {content}", templateDto.Id, fullUrl, requestStr, content);

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function SendMessageTemplateStatusUpdate with received object {object}", ex, JsonConvert.SerializeObject(messageTemplate));
            }
        }
    }
}
