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

        private SendMessageTemplateModel GetTemplateContent(SendMessageTemplateRequestDto model)
        {
            var template = new SendMessageTemplateModel
            {
                messaging_product = "whatsapp",
                to = String.Empty,
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
                        type = "HEADER"
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
                                if (CommonHelper.IsValidUrl(value.Value))
                                {
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.IMAGE.ToLower(),
                                        image = new
                                        {
                                            link = value.Value ?? ""
                                        }
                                    });
                                }
                                else
                                {
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.IMAGE.ToLower(),
                                        image = new
                                        {
                                            id = value.Value ?? ""
                                            //link = value.Value ?? ""
                                        }
                                    });
                                }

                                break;
                            case TemplateHeaderFormatTypeModel.DOCUMENT:
                                if (CommonHelper.IsValidUrl(value.Value))
                                {
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.DOCUMENT.ToLower(),
                                        document = new
                                        {
                                            link = value.Value ?? ""
                                        }
                                    });
                                }
                                else
                                {
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.DOCUMENT.ToLower(),
                                        document = new
                                        {
                                            id = value.Value ?? ""
                                        }
                                    });
                                }

                                break;
                            case TemplateHeaderFormatTypeModel.VIDEO:
                                if (CommonHelper.IsValidUrl(value.Value))
                                {
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.VIDEO.ToLower(),
                                        video = new
                                        {
                                            link = value.Value
                                        }
                                    });
                                }
                                else
                                {
                                    component.parameters.Add(new
                                    {
                                        type = TemplateHeaderFormatTypeModel.VIDEO.ToLower(),
                                        video = new
                                        {
                                            id = value.Value ?? ""
                                        }
                                    });
                                }
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
                        type = "BODY"
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
                            type = "BUTTON",
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

            return template;
        }

        private async Task<UploadMediaResultDto> UploadMedia(string phoneId, UploadMediaDto.MediaDto item)
        {
            _logger.LogInformation("Processing file in function HandleMediaUpload with item {item}", JsonConvert.SerializeObject(item));

            //MediaPath
            var mediaDirectory = String.Concat(_webHostEnvironment.ContentRootPath, "Media");
            if (!Directory.Exists(mediaDirectory))
                Directory.CreateDirectory(mediaDirectory);

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
                    //results.Add(result);
                    //continue;
                }

                HttpResponseMessage headResponses = await _httpClient.GetAsync(item.Url);
                if (!headResponses.IsSuccessStatusCode)
                {
                    _logger.LogError("Error in processing file in function HandleMediaUpload with item {item}, cannot fetch file from origin server", JsonConvert.SerializeObject(item));
                    //results.Add(result);
                    //continue;
                    return result;
                }

                if (!headResponses.Content.Headers.ContentLength.HasValue)
                {
                    _logger.LogError("Error in processing file in function HandleMediaUpload with item {item}, cannot fetch file from origin server", JsonConvert.SerializeObject(item));
                    //results.Add(result);
                    //continue;

                    return result;
                }

                // Create a Uri object
                Uri uri = new Uri(item.Url);

                //string filename = item.Url.Substring(item.Url.LastIndexOf('/') + 1);

                // Get the file name from the Uri
                string filename = Path.GetFileName(uri.LocalPath);

                filepath = Path.Combine(mediaDirectory, filename);

                await using var savefileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                await headResponses.Content.CopyToAsync(savefileStream);

                savefileStream.Close();
                savefileStream.Dispose();

                var request = new HttpRequestMessage(HttpMethod.Post, $"{phoneId}/media");

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

                        //results.Add(result);
                        return result;
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
                //results.Add(result);
                return result;
            }
            finally //Delete the media 
            {
                if (File.Exists(filepath))
                    File.Delete(filepath);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Send batch messages
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<SendMessageResponseDto>> HandleSendBatchMessage(SendMessageRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {
                model.Type = model.Type.ToLower();
                model.PhoneId = model.PhoneId.Trim();
                model.Message = model.Message.Trim();
                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Replace("+", "").Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogInformation("Calling function HandleSendBatchMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);

                var accessToken = await _integrationHandler.GetAccessTokenByClientId(model.ClientId);
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                dynamic messageContent = GetMessageContent(model);
                for (int i = 0; i < batches.Count; i++)
                {
                    string requestStr = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var batch = batches[i];
                        var batchRequest = new BatchMessageRequestModel
                        {
                            batch = batch.Select(recipient => new Batch
                            {
                                method = "POST",
                                relative_url = $"{model.PhoneId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={model.Type}&{model.Type}={JsonConvert.SerializeObject(messageContent)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogInformation("Created Batch Request in function HandleSendBatchMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount} and batchIndex {batchIndex} and batchRequest {batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        // Send the batch request
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendBatchMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount} and batchIndex {batchIndex} and batchRequest {batchRequest} and batchResponse {batchResponse}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr);

                        var responseModel = JsonConvert.DeserializeObject<List<BatchMessageResponseModel>>(responseStr);
                        for (int j = 0; j < batch.Count; j++)
                        {
                            var response = responseModel[j];
                            response.bodyResponse = JsonConvert.DeserializeObject<BatchMessageResponseModel.BodyResponse>(response.body);

                            if (response.code == 200) //If success
                            {
                                var responseDto = new SendMessageResponseDto
                                {
                                    Success = true,
                                    PhoneNumber = batch[j],
                                    WAId = response.bodyResponse.contacts[0].wa_id,
                                    Status = response.code,
                                    MessageId = response.bodyResponse.messages[0].id
                                };

                                models.Add(responseDto);
                            }
                            else
                            {
                                var responseDto = new SendMessageResponseDto
                                {
                                    Success = false,
                                    PhoneNumber = batch[j],
                                    WAId = String.Empty,
                                    Status = response.bodyResponse.error.code,
                                    MessageId = String.Empty,
                                    Errors = new List<string> {
                                        response.bodyResponse.error.message
                                    }
                                };

                                models.Add(responseDto);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchMessage with received object {object} with batch size {batchSize} and batchCount {batchCount} and batchIndex {batchIndex} and batchRequest {batchRequest} and batchResponse {batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchMessage with received object {object}", ex, JsonConvert.SerializeObject(model));
            }

            _logger.LogInformation("Execution ends for function HandleSendBatchMessage with received object {object} and response {response}", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(models));

            return models;
        }

        /// <summary>
        /// Send batch template messages
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<SendMessageResponseDto>> HandleSendBatchTemplateMessage(SendMessageTemplateRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {

                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Replace("+", "").Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogInformation("Calling function HandleSendBatchTemplateMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);

                var template = GetTemplateContent(model);

                var accessToken = await _integrationHandler.GetAccessTokenByClientId(model.ClientId);
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                for (int i = 0; i < batches.Count; i++)
                {
                    string requestStr = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var batch = batches[i];
                        var batchRequest = new BatchMessageRequestModel
                        {
                            batch = batch.Select(recipient => new Batch
                            {
                                method = "POST",
                                relative_url = $"{model.PhoneId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={template.type}&{template.type}={JsonConvert.SerializeObject(template.template)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogInformation("Created Batch Request in function HandleSendBatchTemplateMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount} and batchIndex {batchIndex} and batchRequest {batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        // Send the batch request   
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendBatchTemplateMessage with received object {object} with batch size {batchSize} and totalbatchCount {totalbatchCount} and batchIndex {batchIndex} and batchRequest {batchRequest} and batchResponse {batchResponse}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr);

                        var responseModel = JsonConvert.DeserializeObject<List<BatchMessageResponseModel>>(responseStr);
                        for (int j = 0; j < batch.Count; j++)
                        {
                            var response = responseModel[j];
                            response.bodyResponse = JsonConvert.DeserializeObject<BatchMessageResponseModel.BodyResponse>(response.body);

                            if (response.code == 200) //If success
                            {
                                var responseDto = new SendMessageResponseDto
                                {
                                    Success = true,
                                    PhoneNumber = batch[j],
                                    WAId = response.bodyResponse.contacts[0].wa_id,
                                    Status = response.code,
                                    MessageId = response.bodyResponse.messages[0].id
                                };

                                models.Add(responseDto);
                            }
                            else
                            {
                                var responseDto = new SendMessageResponseDto
                                {
                                    Success = false,
                                    PhoneNumber = batch[j],
                                    WAId = String.Empty,
                                    Status = response.bodyResponse.error.code,
                                    MessageId = String.Empty,
                                    Errors = new List<string> {
                                        response.bodyResponse.error.message
                                    }
                                };

                                models.Add(responseDto);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchTemplateMessage with received object {object} with batch size {batchSize} and batchCount {batchCount} and batchIndex {batchIndex} and batchRequest {batchRequest} and batchResponse {batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchTemplateMessage with received object {object}", ex, JsonConvert.SerializeObject(model));
            }

            _logger.LogInformation("Execution ends for function HandleSendBatchTemplateMessage with received object {object} and response {response}", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(models));

            return models;
        }

        /// <summary>
        /// Handle media upload
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>
        public async Task<List<UploadMediaResultDto>> HandleMediaUpload(UploadMediaDto model)
        {
            _logger.LogInformation("Calling function HandleMediaUpload with received payload {payload}", JsonConvert.SerializeObject(model));

            var accessToken = await _integrationHandler.GetAccessTokenByClientId(model.ClientId);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            List<UploadMediaResultDto> results = new List<UploadMediaResultDto>();
            foreach (var item in model.Medias)
            {
                var media = await UploadMedia(model.PhoneId, item);
                results.Add(media);
            }

            return results;
        }

        #endregion
    }
}
