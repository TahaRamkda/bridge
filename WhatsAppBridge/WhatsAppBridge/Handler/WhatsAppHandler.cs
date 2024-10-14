using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Models.WhatsApp.Types;
using WhatsAppBridge.Settings;
using static WhatsAppBridge.Models.WhatsApp.BatchMessageRequestModel;

namespace WhatsAppBridge.Handler
{
    public partial class WhatsAppHandler
    {
        #region Fields

        private readonly ILogger<WhatsAppHandler> _logger;
        private readonly IOptions<WhatsAppConfigurationSetting> _whatsAppConfigurationSetting;
        private readonly IntegrationHandler _integrationHandler;
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = String.Empty;
        private readonly IWebHostEnvironment _webHostEnvironment;

        #endregion

        #region Ctor

        public WhatsAppHandler(ILogger<WhatsAppHandler> logger,
         IOptions<WhatsAppConfigurationSetting> whatsAppConfigurationSetting,
         IntegrationHandler integrationHandler,
         IHttpClientFactory httpClientFactory,
         IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _whatsAppConfigurationSetting = whatsAppConfigurationSetting;
            _integrationHandler = integrationHandler;
            _httpClient = httpClientFactory.CreateClient(HttpClientType.facebook_graph_api);
            baseUrl = _httpClient.BaseAddress.AbsoluteUri;
            _webHostEnvironment = webHostEnvironment;
        }

        #endregion

        #region Utilities

        private dynamic GetMessageContent(SendMessageRequestDto model)
        {
            dynamic messageContent = null;
            switch (model.Type.ToLower())
            {
                case MessageType.TEXT:
                    messageContent = new
                    {
                        preview_url = false,
                        body = model.Message
                    };

                    break;

                case MessageType.IMAGE:
                    messageContent = new
                    {
                        id = model.MediaId,
                        caption = model.Message
                    };

                    break;
                case MessageType.DOCUMENT:
                    messageContent = new
                    {
                        id = model.MediaId,
                        caption = model.Message,
                        filename = model.FileName
                    };

                    break;
                default:
                    break;
            }

            return messageContent;
        }

        #endregion

        #region Methods

        public async Task<List<SendMessageResponseDto>> HandleSendMessage(SendMessageRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {
                model.Type = model.Type.ToLower();
                model.PhoneId = model.PhoneId.Trim();
                model.Message = model.Message.Trim();
                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogInformation("Calling function HandleSendMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);
                  
                dynamic messageContent = GetMessageContent(model);

                for (int i = 0; i < batches.Count; i++)
                {
                    string requestStr = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var batchRequest = new BatchMessageRequestModel
                        {
                            batch = batches[i].Select(recipient => new Batch
                            {
                                method = "POST",
                                relative_url = $"{model.PhoneId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={model.Type}&{model.Type}={JsonConvert.SerializeObject(messageContent)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogInformation("Created Batch Request in function HandleSendMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount} and batchIndex {batchIndex} and batchRequest {batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        // Send the batch request
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount} and batchIndex {batchIndex} and batchRequest {batchRequest} and batchResponse {batchResponse}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr);

                        var responseModel = JsonConvert.DeserializeObject<List<BatchMessageResponseModel>>(responseStr);
                        foreach (var response in responseModel)
                        {
                            if (response.code == 200) //If success
                            {
                                response.bodyResponse = JsonConvert.DeserializeObject<BatchMessageResponseModel.BodyResponse>(response.body);
                                var responseDto = new SendMessageResponseDto
                                {
                                    PhoneNumber = response.bodyResponse.contacts[0].input,
                                    WAId = response.bodyResponse.contacts[0].wa_id,
                                    Status = response.code,
                                    MessageId = response.bodyResponse.messages[0].id
                                };

                                models.Add(responseDto);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendMessage with received object {object} with batch size {batchSize} and batchCount {batchCount} and batchIndex {batchIndex} and batchRequest {batchRequest} and batchResponse {batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendMessage with received object {object}", ex, JsonConvert.SerializeObject(model));
            }

            return models;
        }

        public async Task<List<UploadMediaResultDto>> HandleMediaUpload(UploadMediaDto model)
        {
            _logger.LogInformation("Calling function HandleMediaUpload with received payload {payload}", JsonConvert.SerializeObject(model));

            //MediaPath
            var mediaDirectory = String.Concat(_webHostEnvironment.ContentRootPath, "Media");
            if (!Directory.Exists(mediaDirectory))
                Directory.CreateDirectory(mediaDirectory);

            List<UploadMediaResultDto> results = new List<UploadMediaResultDto>();
            foreach (var item in model.Medias)
            {
                _logger.LogInformation("Processing file in function HandleMediaUpload with item {item}", JsonConvert.SerializeObject(item));

                string filepath = String.Empty;
                string responseStr = String.Empty;
                UploadMediaResultDto result = new UploadMediaResultDto
                {
                    Id = item.Id
                };

                try
                {
                    if (String.IsNullOrWhiteSpace(item.Url))
                    {
                        _logger.LogError("Error in processing file in function HandleMediaUpload with item {item}, item does not have URL", JsonConvert.SerializeObject(item));
                        results.Add(result);
                        continue;
                    }

                    HttpResponseMessage headResponses = await _httpClient.GetAsync(item.Url);
                    if (!headResponses.IsSuccessStatusCode)
                    {
                        _logger.LogError("Error in processing file in function HandleMediaUpload with item {item}, cannot fetch file from origin server", JsonConvert.SerializeObject(item));
                        results.Add(result);
                        continue;
                    }

                    if (!headResponses.Content.Headers.ContentLength.HasValue)
                    {
                        _logger.LogError("Error in processing file in function HandleMediaUpload with item {item}, cannot fetch file from origin server", JsonConvert.SerializeObject(item));
                        results.Add(result);
                        continue;
                    }

                    string filename = item.Url.Substring(item.Url.LastIndexOf('/') + 1);
                    filepath = Path.Combine(mediaDirectory, filename);

                    await using var savefileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                    await headResponses.Content.CopyToAsync(savefileStream);

                    savefileStream.Close();
                    savefileStream.Dispose();

                    var request = new HttpRequestMessage(HttpMethod.Post, $"{model.PhoneId}/media");

                    // Prepare file content
                    using (var content = new MultipartFormDataContent())
                    {
                        var fileInfo = new FileInfo(filepath);

                        string contentType = String.Empty;
                        new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FullName, out contentType);

                        // Read the file from the local path
                        var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                        var fileContent = new StreamContent(fileStream);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                        //fileContent.Headers.ContentType = new MediaTypeHeaderValue($"image/{fileType}");

                        // Add file content to the form-data
                        content.Add(fileContent, "file", Path.GetFileName(filepath));

                        // Add other form data parameters
                        content.Add(new StringContent("whatsapp"), "messaging_product");

                        // Add form data type
                        content.Add(new StringContent(contentType), "type");

                        // Assign content to the request
                        request.Content = content;

                        // Send the request and get the response
                        var response = await _httpClient.SendAsync(request);

                        // Read the response content
                        responseStr = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            var fileJsonResponse = JObject.Parse(responseStr);
                            var fileResponse = fileJsonResponse["id"]?.ToString();
                            result.MediaId = fileResponse;

                            results.Add(result);
                        }
                        else
                        {
                            _logger.LogError("Error in processing file in function HandleMediaUpload with item {item}, cannot upload file with response {response}", JsonConvert.SerializeObject(item), responseStr);
                            throw new BadHttpRequestException(responseStr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occurred {exception} when executing function HandleMediaUpload with item {item} and response {responseStr }", ex, JsonConvert.SerializeObject(item), responseStr);
                    results.Add(result);
                }
                finally //Delete the media 
                {
                    if (File.Exists(filepath))
                        File.Delete(filepath);
                }
            }

            return results;
        }

        public async Task HandleSendTemplateMessage(SendMessageTemplateRequestDto model)
        {
            _logger.LogInformation("Calling function HandleSendTemplateMessage with received payload {payload}", JsonConvert.SerializeObject(model));

            string requestStr = String.Empty;
            string responseStr = String.Empty;

            try
            {
                var template = new SendMessageTemplateModel
                {
                    messaging_product = "whatsapp",
                    to = model.PhoneNumber.Replace("+", "").Trim(),
                    recipient_type = "individual",
                    type = "template",
                    template = new SendMessageTemplateModel.Template
                    {
                        name = model.TemplateName.Trim(),
                        language = new SendMessageTemplateModel.Template.Language
                        {
                            code = model.LanguageCode
                        }
                    }
                };

                foreach (var obj in model.Components)
                {
                    //HEADER TYPE COMPONENT
                    if (obj.ComponentType.ToUpper() == TemplateComponentTypeModel.HEADER.ToUpper())
                    {
                        var component = new SendMessageTemplateModel.Template.Component
                        {
                            type = obj.ComponentType
                        };

                        foreach (var value in obj.Values.OrderBy(x => x.Index))
                        {
                            var valueType = value.Type.ToUpper();
                            switch (valueType)
                            {
                                case TemplateHeaderFormatTypeModel.NONE:
                                    break;
                                case TemplateHeaderFormatTypeModel.TEXT:
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.TEXT.ToLower(),
                                        text = value.Value ?? ""
                                    });
                                    break;
                                case TemplateHeaderFormatTypeModel.IMAGE:
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.IMAGE.ToLower(),
                                        image = new
                                        {
                                            id = value.Value ?? ""
                                        }
                                    });
                                    break;
                                case TemplateHeaderFormatTypeModel.DOCUMENT:
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.DOCUMENT.ToLower(),
                                        document = new
                                        {
                                            id = value.Value ?? ""
                                        }
                                    });
                                    break;
                                case TemplateHeaderFormatTypeModel.VIDEO:
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.VIDEO.ToLower(),
                                        video = new
                                        {
                                            id = value.Value ?? ""
                                        }
                                    });
                                    break;
                                default:
                                    break;
                            }
                        }

                        template.template.components.Add(component);
                    }
                    else if (obj.ComponentType.ToUpper() == TemplateComponentTypeModel.BODY.ToUpper())
                    {
                        var component = new SendMessageTemplateModel.Template.Component
                        {
                            type = obj.ComponentType
                        };

                        foreach (var value in obj.Values.OrderBy(x => x.Index))
                        {
                            component.parameters.Add(new
                            {
                                type = TemplateHeaderFormatTypeModel.TEXT.ToLower(),
                                text = value.Value ?? ""
                            });
                        }

                        template.template.components.Add(component);
                    }
                    else if (obj.ComponentType.ToUpper() == TemplateComponentTypeModel.BUTTONS.ToUpper()
                        || obj.ComponentType.ToUpper() == TemplateComponentTypeModel.BUTTON.ToUpper())
                    {
                        foreach (var value in obj.Values.OrderBy(x => x.Index))
                        {
                            //For Button we pass as index
                            var component = new SendMessageTemplateModel.Template.Component
                            {
                                type = obj.ComponentType,
                                sub_type = value.Type,
                                index = value.Index
                            };

                            var valueType = value.Type.ToUpper();
                            switch (valueType)
                            {
                                case TemplateButtonTypeModel.QUICK_REPLY:
                                    component.parameters.Add(new
                                    {
                                        //type = TemplateButtonTypeModel.QUICK_REPLY.ToLower(),
                                        type = "text",
                                        text = value.Value ?? ""
                                    });
                                    break;
                                case TemplateButtonTypeModel.PHONE_NUMBER:
                                    component.parameters.Add(new
                                    {
                                        type = "text",
                                        //index = value.Index,
                                        text = value.Value ?? ""
                                    });
                                    break;
                                case TemplateButtonTypeModel.URL:
                                    component.parameters.Add(new
                                    {
                                        type = "text",
                                        //index = value.Index,
                                        text = value.Value ?? ""
                                    });
                                    break;

                                default:
                                    break;
                            }

                            template.template.components.Add(component);
                        }
                    }
                }

                requestStr = JsonConvert.SerializeObject(template);

                // Send the batch request
                var resp = await _httpClient.PostAsync($"{model.PhoneId}/messages", new StringContent(requestStr, null, "application/json"));
                responseStr = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("Received Response in function HandleSendTemplateMessage with received object {object} and requestStr {requestStr} and responseStr {responseStr}", JsonConvert.SerializeObject(model), requestStr, responseStr);

                var response = JsonConvert.DeserializeObject<BatchMessageResponseModel.BodyResponse>(responseStr);
                var responseDto = new SendMessageResponseDto
                {
                    PhoneNumber = response.contacts[0].input,
                    WAId = response.contacts[0].wa_id,
                    Status = 200,
                    MessageId = response.messages[0].id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendTemplateMessage with received object {object}", ex, JsonConvert.SerializeObject(model));
            }
        }

        #endregion
    }
}
