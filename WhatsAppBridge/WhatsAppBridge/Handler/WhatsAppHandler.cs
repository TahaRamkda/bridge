using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.Integration.Flow;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Models.WhatsApp.Flow;
using WhatsAppBridge.Models.WhatsApp.Types;
using WhatsAppBridge.Settings;

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
            model.Type = model.Type ?? "";
            model.Type = model.Type.ToLower() == "none" ? "text" : model.Type; //If none, convert it to text
            switch (model.Type.ToLower())
            {
                case MessageType.TEXT:
                    messageContent = new
                    {
                        preview_url = false,
                        body = model.Message.EncodeSpecialCharacters()
                    };

                    break;

                case MessageType.IMAGE:
                    messageContent = new
                    {
                        id = model.MediaId.EncodeSpecialCharacters(),
                        caption = model.Message.EncodeSpecialCharacters()
                    };

                    break;

                case MessageType.AUDIO:
                    messageContent = new
                    {
                        id = model.MediaId.EncodeSpecialCharacters()
                    };

                    break;

                case MessageType.VIDEO:
                    messageContent = new
                    {
                        id = model.MediaId.EncodeSpecialCharacters(),
                        caption = model.Message.EncodeSpecialCharacters()
                    };

                    break;
                case MessageType.DOCUMENT:
                    messageContent = new
                    {
                        id = model.MediaId.EncodeSpecialCharacters(),
                        caption = model.Message.EncodeSpecialCharacters(),
                        filename = model.FileName.EncodeSpecialCharacters()
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
                    name = model.TemplateName.EncodeSpecialCharacters(),
                    language = new SendMessageTemplateModel.Template.Language
                    {
                        code = model.LanguageCode.EncodeSpecialCharacters()
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
                                    text = value.Value.EncodeSpecialCharacters()
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
                                            link = value.Value.EncodeSpecialCharacters()
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
                                            id = value.Value.EncodeSpecialCharacters()
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
                                            link = value.Value.EncodeSpecialCharacters()
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
                                            id = value.Value.EncodeSpecialCharacters()
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
                                            link = value.Value.EncodeSpecialCharacters()
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
                                            id = value.Value.EncodeSpecialCharacters()
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
                            text = value.Value.EncodeSpecialCharacters()
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
                                    text = value.Value.EncodeSpecialCharacters()
                                });
                                break;
                            case TemplateButtonTypeModel.PHONE_NUMBER:
                                component.parameters.Add(new
                                {
                                    type = "text",
                                    text = value.Value.EncodeSpecialCharacters()
                                });
                                break;
                            case TemplateButtonTypeModel.URL:
                                component.parameters.Add(new
                                {
                                    type = "text",
                                    text = value.Value.EncodeSpecialCharacters()
                                });
                                break;

                            default:
                                break;
                        }

                        template.template.components.Add(component);
                    }
                }
            }

            if (model.FlowAction != null)
            {
                //For Button we pass as index
                var component = new SendMessageTemplateModel.Template.Component
                {
                    type = "BUTTON",
                    sub_type = "flow",
                    index = model.FlowAction.Index
                };

                component.parameters.Add(new
                {
                    //type = TemplateButtonTypeModel.QUICK_REPLY.ToLower(),
                    type = "action",
                    action = new
                    {
                        flow_token = model.FlowAction.Token
                    }
                });

                template.template.components.Add(component);
            }

            return template;
        }

        private SendInteractiveMessageModel GetInteractiveMessageContent(SendInteractiveMessageRequestDto model)
        {
            var interactive = new SendInteractiveMessageModel
            {
                messaging_product = "whatsapp",
                to = String.Empty,
                recipient_type = "individual",
                type = "interactive",
                interactive = new ExpandoObject()
            };

            //HEADER
            if (model.Header != null && model.Header.Format != TemplateHeaderFormatTypeModel.NONE)
            {
                model.Header.Format = model.Header.Format.ToUpper();

                //HEADER TYPE COMPONENT
                dynamic header = new ExpandoObject();
                if (model.Header.Format == TemplateHeaderFormatTypeModel.TEXT)
                {
                    header.type = TemplateHeaderFormatTypeModel.TEXT;
                    header.text = model.Header.Value.EncodeSpecialCharacters();
                }
                else if (model.Header.Format == TemplateHeaderFormatTypeModel.IMAGE)
                {
                    header.type = TemplateHeaderFormatTypeModel.IMAGE;
                    if (CommonHelper.IsValidUrl(model.Header.Value))
                    {
                        header.image = new
                        {
                            link = model.Header.Value.EncodeSpecialCharacters()
                        };
                    }
                    else
                    {
                        header.image = new
                        {
                            id = model.Header.Value.EncodeSpecialCharacters()
                        };
                    }
                }
                else if (model.Header.Format == TemplateHeaderFormatTypeModel.DOCUMENT)
                {
                    header.type = TemplateHeaderFormatTypeModel.DOCUMENT;
                    if (CommonHelper.IsValidUrl(model.Header.Value))
                    {
                        header.document = new
                        {
                            link = model.Header.Value.EncodeSpecialCharacters()
                        };
                    }
                    else
                    {
                        header.document = new
                        {
                            id = model.Header.Value.EncodeSpecialCharacters()
                        };
                    }
                }
                else if (model.Header.Format == TemplateHeaderFormatTypeModel.VIDEO)
                {
                    header.type = TemplateHeaderFormatTypeModel.VIDEO;
                    if (CommonHelper.IsValidUrl(model.Header.Value))
                    {
                        header.video = new
                        {
                            link = model.Header.Value.EncodeSpecialCharacters()
                        };
                    }
                    else
                    {
                        header.video = new
                        {
                            id = model.Header.Value.EncodeSpecialCharacters()
                        };
                    }
                }

                interactive.interactive.header = header;
            }

            //BODY
            if (model.Body != null && !String.IsNullOrWhiteSpace(model.Body.Text))
            {
                interactive.interactive.body = new
                {
                    text = model.Body.Text.EncodeSpecialCharacters()
                };
            }

            //FOOTER
            if (model.Footer != null && !String.IsNullOrWhiteSpace(model.Footer.Text))
            {
                interactive.interactive.footer = new
                {
                    text = model.Footer.Text.EncodeSpecialCharacters()
                };
            }

            //BUTTONS
            if (model.AskForLocation)
            {
                interactive.interactive.type = "location_request_message";
                interactive.interactive.action = new
                {
                    name = "send_location"
                };
            }
            else if (model.FlowAction != null) //FLOW
            {
                interactive.interactive.type = "flow";
                interactive.interactive.action = new
                {
                    name = "flow",
                    parameters = new
                    {
                        flow_message_version = model.FlowAction.Version,
                        flow_id = model.FlowAction.FlowId,
                        flow_cta = model.FlowAction.ButtonText,
                        flow_token = model.FlowAction?.Token
                    }
                };
            }
            else if (model.Buttons != null && model.Buttons.Count() > 0)
            {
                if (model.Buttons.Any(x => x.Type.ToUpper() == TemplateButtonTypeModel.URL)) //BUTTON URL
                {
                    interactive.interactive.type = "cta_url";
                    var button = model.Buttons.FirstOrDefault(x => x.Type.ToUpper() == TemplateButtonTypeModel.URL);

                    interactive.interactive.action = new
                    {
                        name = "cta_url",
                        parameters = new
                        {
                            display_text = button.Text.EncodeSpecialCharacters(),
                            url = button.Url.EncodeSpecialCharacters()
                        }
                    };
                }
                else if (model.Buttons.Count <= 3) //BUTTON LIST WITH LESS THAN OR EQUAL TO 3 BUTTONS
                {
                    interactive.interactive.type = "button";
                    interactive.interactive.action = new
                    {
                        buttons = new List<object>()
                    };

                    foreach (var button in model.Buttons)
                    {
                        interactive.interactive.action.buttons.Add(new
                        {
                            type = "reply",
                            reply = new
                            {
                                id = button.Id.EncodeSpecialCharacters(),
                                title = button.Text.EncodeSpecialCharacters()
                            }
                        });
                    }
                }
                else if (model.Buttons.Count > 3) //BUTTON LIST WITH MORE THAN 3 BUTTONS
                {
                    dynamic rows = new List<object>();
                    foreach (var button in model.Buttons)
                    {
                        rows.Add(new
                        {
                            id = button.Id.EncodeSpecialCharacters(),
                            title = button.Text.EncodeSpecialCharacters()
                        });
                    }

                    interactive.interactive.type = "list";
                    interactive.interactive.action = new
                    {
                        button = "Choose options",
                        sections = new List<object> { new { rows = rows } }
                    };
                }
            }
              
            return interactive;
        }

        private async Task<UploadMediaResultDto> UploadMedia(string phoneId, UploadMediaDto.MediaDto item)
        {
            _logger.LogDebug("Processing file in function HandleMediaUpload with item={item}", JsonConvert.SerializeObject(item));

            //MediaPath
            var mediaDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "Media");
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
                    _logger.LogError("Error in processing file in function HandleMediaUpload with item={item}, item does not have URL", JsonConvert.SerializeObject(item));
                    //results.Add(result);
                    //continue;
                }

                HttpResponseMessage headResponses = await _httpClient.GetAsync(item.Url);
                if (!headResponses.IsSuccessStatusCode)
                {
                    _logger.LogError("Error in processing file in function HandleMediaUpload with item={item}, cannot fetch file from origin server", JsonConvert.SerializeObject(item));
                    //results.Add(result);
                    //continue;
                    return result;
                }

                if (!headResponses.Content.Headers.ContentLength.HasValue)
                {
                    _logger.LogError("Error in processing file in function HandleMediaUpload with item={item}, cannot fetch file from origin server", JsonConvert.SerializeObject(item));
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
                        _logger.LogError("Error in processing file in function HandleMediaUpload with item={item}, cannot upload file with response={response}", JsonConvert.SerializeObject(item), responseStr);
                        throw new BadHttpRequestException(responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleMediaUpload with item={item} and response={responseStr}", ex, JsonConvert.SerializeObject(item), responseStr);
                //results.Add(result);
                return result;
            }
            finally //Delete the media 
            {
                if (File.Exists(filepath))
                    File.Delete(filepath);
            }
        }

        private async Task<string> UploadMediaAsset(string phoneId, string accessToken, string appId, string url)
        {
            string sessionId = String.Empty;
            string fileHandleId = String.Empty;
            string filepath = String.Empty;

            try
            {
                _logger.LogDebug("Processing file in function UploadMediaAsset with input url={url}", url);

                if (String.IsNullOrWhiteSpace(url))
                {
                    _logger.LogError("Error in processing file in function UploadMediaAsset with url={url}, item does not have URL", url);
                }

                //Dirty fix, will rectify in API solution
                url = url.Replace("\\", "/");

                //MediaPath
                var mediaDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "MediaAssets");
                if (!Directory.Exists(mediaDirectory))
                    Directory.CreateDirectory(mediaDirectory);

                HttpResponseMessage headResponses = await _httpClient.GetAsync(url);
                if (!headResponses.Content.Headers.ContentLength.HasValue)
                {
                    _logger.LogError("Error in processing file in function UploadMediaAsset with url={url}, cannot fetch file from origin server", url);
                    return String.Empty;
                }

                // Create a Uri object
                Uri uri = new Uri(url);

                // Get the file name from the Uri
                string filename = Path.GetFileName(uri.LocalPath);

                filepath = Path.Combine(mediaDirectory, filename);

                await using var savefileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                await headResponses.Content.CopyToAsync(savefileStream);
                FileInfo fileInfo = new FileInfo(filepath);
                var fileLength = fileInfo.Length;

                string contentType = String.Empty;
                new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FullName, out contentType);

                savefileStream.Close();
                savefileStream.Dispose();

                var uploadSessionResponse = await _httpClient.PostAsync($"{appId}/uploads" +
                    $"?file_name={filename}&file_length={fileLength}&file_type={contentType}&access_token={accessToken}", new StringContent(string.Empty));

                var uploadSessionResponseContent = await uploadSessionResponse.Content.ReadAsStringAsync();
                if (!uploadSessionResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error in processing file in function UploadMediaAsset with url={url} and response={response}, doesn't received upload session id from facebook", url, uploadSessionResponseContent);
                    return String.Empty;
                }

                var jsonResponse = JObject.Parse(uploadSessionResponseContent);
                sessionId = jsonResponse["id"]?.ToString();

                if (String.IsNullOrWhiteSpace(sessionId))
                {
                    _logger.LogError("Error in processing file in function UploadMediaAsset with url={url} and response={response}, doesn't received upload session id from facebook", url, uploadSessionResponseContent);
                    return String.Empty;
                }

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_whatsAppConfigurationSetting.Value.BaseURL}/{sessionId}");

                request.Headers.Clear();
                request.Headers.Add("authorization", $"OAuth {accessToken}");
                request.Headers.Add("file_offset", "0");

                // Prepare file content
                using (var content = new MultipartFormDataContent())
                {
                    // Read the file from the local path
                    var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    // Add file content to the form-data
                    content.Add(fileContent, "data-binary");

                    // Assign content to the request
                    request.Content = content;

                    // Send the request and get the response
                    var uploadfileResponse = await _httpClient.SendAsync(request);
                    var uploadFileResponseContent = await uploadfileResponse.Content.ReadAsStringAsync();

                    if (!uploadfileResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Error in processing file in function UploadMediaAsset with url={url} and sessionId={sessionId} and response={response}, doesn't received upload session id from facebook", url, sessionId, uploadSessionResponseContent);
                        return String.Empty;
                    }

                    var fileJsonResponse = JObject.Parse(uploadFileResponseContent);
                    fileHandleId = fileJsonResponse["h"]?.ToString();
                    return fileHandleId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function UploadMediaAsset with url={url} and sessionId={sessionId} and fileHandleId={fileHandleId}", ex, url, sessionId, fileHandleId);
            }
            finally // Delete the media
            {
                if (File.Exists(filepath))
                    File.Delete(filepath);
            }

            return String.Empty;
        }

        private SendMessageCarouselTemplateModel GetCarouselTemplateContent(SendMessageCarouselRequestDto model)
        {
            var template = new SendMessageCarouselTemplateModel
            {
                messaging_product = "whatsapp",
                to = String.Empty,
                recipient_type = "individual",
                type = "template",
                template = new SendMessageCarouselTemplateModel.Template
                {
                    name = model.TemplateName.EncodeSpecialCharacters(),
                    language = new SendMessageCarouselTemplateModel.Template.Language
                    {
                        code = model.LanguageCode.EncodeSpecialCharacters()
                    },
                    components = new List<object>()
                }
            };

            //Add body parameters
            if (model.BodyParams != null && model.BodyParams.Any())
            {
                template.template.components.Add(new
                {
                    type = "body",
                    parameters = model.BodyParams.Select(p => new { type = TemplateHeaderFormatTypeModel.TEXT.ToLower(), text = p })
                });
            }

            if (model.Cards != null && model.Cards.Any())
            {
                var carouselComp = new
                {
                    type = "carousel",
                    cards = new List<object>()
                };

                foreach (var card in model.Cards.OrderBy(x => x.Index))
                {
                    var cardObj = new
                    {
                        card_index = card.Index,
                        components = new List<object>()
                    };

                    foreach (var obj in card.Components)
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
                                            text = value.Value.EncodeSpecialCharacters()
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
                                                    link = value.Value.EncodeSpecialCharacters()
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
                                                    id = value.Value.EncodeSpecialCharacters()
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
                                                    link = value.Value.EncodeSpecialCharacters()
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
                                                    id = value.Value.EncodeSpecialCharacters()
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
                                                    link = value.Value.EncodeSpecialCharacters()
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
                                                    id = value.Value.EncodeSpecialCharacters()
                                                }
                                            });
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }

                            cardObj.components.Add(component);
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
                                    text = value.Value.EncodeSpecialCharacters()
                                });
                            }

                            cardObj.components.Add(component);
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
                                            text = value.Value.EncodeSpecialCharacters()
                                        });
                                        break;
                                    case TemplateButtonTypeModel.PHONE_NUMBER:
                                        component.parameters.Add(new
                                        {
                                            type = "text",
                                            text = value.Value.EncodeSpecialCharacters()
                                        });
                                        break;
                                    case TemplateButtonTypeModel.URL:
                                        component.parameters.Add(new
                                        {
                                            type = "text",
                                            text = value.Value.EncodeSpecialCharacters()
                                        });
                                        break;

                                    default:
                                        break;
                                }

                                cardObj.components.Add(component);
                            }
                        }
                    }

                    carouselComp.cards.Add(cardObj);
                }

                template.template.components.Add(carouselComp);
            }

            return template;
        }

        #endregion

        #region Methods

        #region Messages

        /// <summary>
        /// Send batch messages
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> HandleSendBatchMessage(SendMessageRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {
                model.Type = model.Type.ToLower();
                //model.PhoneId = model.PhoneId.Trim();
                model.Message = (model.Message ?? "").Trim();
                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Replace("+", "").Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogDebug("Calling function HandleSendBatchMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);

                var senderInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderInfo == null)
                {
                    return new ApiResult
                    {
                        StatusCode = 404,
                        Message = $"Sender not found with clientId: {model.ClientId} and senderNameId:{model.SenderNameId}"
                    };
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderInfo.AccessToken}");

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
                            batch = batch.Select(recipient => new BatchMessageRequestModel.Batch
                            {
                                method = "POST",
                                relative_url = $"{senderInfo.PhoneNumberId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={model.Type}&{model.Type}={JsonConvert.SerializeObject(messageContent)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogDebug("Created Batch Request in function HandleSendBatchMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        var apiCallStart = DateTime.UtcNow;

                        // Send the batch request
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendBatchMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse} with apiResponseTime={apiResponseTime}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

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
                                    PhoneNumber = response.bodyResponse.contacts[0].wa_id,
                                    WAId = response.bodyResponse.messages[0].id,
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
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchMessage with received object={object} with batch size={batchSize} and batchCount={batchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchMessage with received object={object}", ex, JsonConvert.SerializeObject(model));
            }

            _logger.LogDebug("Execution ends for function HandleSendBatchMessage with received object={object} and response={response}", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(models));

            if (!models.Any())
            {
                return new ApiResult
                {
                    StatusCode = 400,
                    Message = "Couldn't send messages",
                    Result = models
                };
            }

            return new ApiResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Data processed succesfully",
                Result = models
            };
        }

        /// <summary>
        /// Send interactive message
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <summary>
        /// Send interactive message
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> HandleSendInteractiveMessage(SendInteractiveMessageRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {
                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Replace("+", "").Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogDebug("Calling function HandleSendInteractiveMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);

                var interactive = GetInteractiveMessageContent(model);

                var senderInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderInfo == null)
                {
                    return new ApiResult
                    {
                        StatusCode = 404,
                        Message = $"Client not found with clientId: {model.ClientId}"
                    };
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderInfo.AccessToken}");

                for (int i = 0; i < batches.Count; i++)
                {
                    string requestStr = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var batch = batches[i];
                        var batchRequest = new BatchMessageRequestModel
                        {
                            batch = batch.Select(recipient => new BatchMessageRequestModel.Batch
                            {
                                method = "POST",
                                relative_url = $"{senderInfo.PhoneNumberId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={interactive.type}&{interactive.type}={JsonConvert.SerializeObject(interactive.interactive)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogDebug("Created Batch Request in function HandleSendInteractiveMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        var apiCallStart = DateTime.UtcNow;

                        // Send the batch request   
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendInteractiveMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse} with apiResponseTime={apiResponseTime}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

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
                                    PhoneNumber = response.bodyResponse.contacts[0].wa_id,
                                    WAId = response.bodyResponse.messages[0].id,
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
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendInteractiveMessage with received object={object} with batch size={batchSize} and batchCount={batchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendInteractiveMessage with received object={object}", ex, JsonConvert.SerializeObject(model));
            }

            _logger.LogDebug("Execution ends for function HandleSendInteractiveMessage with received object={object} and response={response}", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(models));

            if (!models.Any())
            {
                return new ApiResult
                {
                    StatusCode = 400,
                    Message = "Couldn't send messages",
                    Result = models
                };
            }

            return new ApiResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Data processed succesfully",
                Result = models
            };
        }

        #endregion

        #region Templates

        /// <summary>
        /// Handle message template OPS
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>
        public async Task<ApiResult> HandleMessageTemplateOps(CreateMessageTemplateRequestDto model)
        {
            string requestStr = String.Empty;
            string responseStr = String.Empty;

            try
            {
                //Replace empty spaces in template name with _
                model.Name = (model.Name ?? "").Replace(" ", "_");

                _logger.LogDebug("Calling function HandleMessageTemplateOps with received payload={payload}", JsonConvert.SerializeObject(model));

                var senderNameInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderNameInfo == null)
                    return new ApiResult { StatusCode = 404, Message = $"Sender name not found with clientId: {model.ClientId} and senderNameId: {model.SenderNameId}" };

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderNameInfo.AccessToken}");

                string messageTemplateId = String.Empty;
                var resp = await _httpClient.GetAsync($"/{senderNameInfo.BusinessAccountId}/message_templates?fields=name,id,status,language&name={model.Name}&language={model.LanguageCode}&limit=1");
                responseStr = await resp.Content.ReadAsStringAsync();

                var messageTemplates = JsonConvert.DeserializeObject<MessageTemplateListResponse>(responseStr);
                if (messageTemplates != null && messageTemplates.data != null && messageTemplates.data.Any())
                {
                    var existingTemplate = messageTemplates.data[0];
                    if (existingTemplate.name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase)
                        && existingTemplate.language.Equals(model.LanguageCode, StringComparison.InvariantCultureIgnoreCase))
                        messageTemplateId = messageTemplates.data[0].id;
                }

                var template = new CreateMessageTemplateRequestModel
                {
                    name = model.Name.Replace(" ", "_").Trim(),
                    category = model.Category.Trim(),
                    language = model.LanguageCode.Trim(),
                    allow_category_change = false
                };

                //Add Header
                if (model.Header != null && model.Header.Format != TemplateHeaderFormatTypeModel.NONE)
                {
                    model.Header.Format = model.Header.Format.Trim().ToUpper();
                    dynamic header = new ExpandoObject();

                    header.type = "HEADER";
                    header.format = model.Header.Format;

                    if (model.Header.Format == TemplateHeaderFormatTypeModel.TEXT)
                    {
                        header.text = model.Header.Text;

                        if (!String.IsNullOrWhiteSpace(model.Header.Example))
                        {
                            header.example = new
                            {
                                header_text = new List<string> { model.Header.Example.Trim() }
                            };
                        }
                    }

                    if (model.Header.Format == TemplateHeaderFormatTypeModel.IMAGE
                        || model.Header.Format == TemplateHeaderFormatTypeModel.VIDEO
                        || model.Header.Format == TemplateHeaderFormatTypeModel.DOCUMENT)
                    {
                        var fileHandleId = await UploadMediaAsset(senderNameInfo.PhoneNumberId, senderNameInfo.AccessToken, senderNameInfo.AppId, model.Header.MediaUrl);

                        if (String.IsNullOrWhiteSpace(fileHandleId))
                            return new ApiResult { StatusCode = 404, Message = $"Cannot uploaded provided header media with url {model.Header.MediaUrl}" };

                        header.example = new
                        {
                            header_handle = new List<string>
                            {
                               fileHandleId
                            }
                        };
                    }

                    template.components.Add(header);
                }

                //Add Body
                if (model.Body != null && !String.IsNullOrWhiteSpace(model.Body.Text))
                {
                    dynamic body = new ExpandoObject();

                    body.type = "BODY";
                    body.text = model.Body.Text.Trim();

                    if (model.Body.Examples.Any())
                    {
                        body.example = new
                        {
                            body_text = new List<object>
                            {
                                model.Body.Examples
                            }
                        };
                    }

                    template.components.Add(body);
                }

                //Add Footer
                if (model.Footer != null && !String.IsNullOrWhiteSpace(model.Footer.Text))
                {
                    dynamic footer = new ExpandoObject();

                    footer.type = "FOOTER";
                    footer.text = model.Footer.Text.Trim();

                    template.components.Add(footer);
                }

                //Add Buttons or Flow
                if (model.Buttons.Any() || model.Flow != null)
                {
                    dynamic buttons = new ExpandoObject();
                    buttons.type = "BUTTONS";
                    buttons.buttons = new List<dynamic>();

                    if (model.Buttons != null)
                    {
                        foreach (var button in model.Buttons)
                        {
                            button.Type = button.Type.Trim().ToUpper();
                            if (button.Type == TemplateButtonTypeModel.QUICK_REPLY && !String.IsNullOrWhiteSpace(button.Text))
                            {
                                dynamic buttonObj = new ExpandoObject();

                                buttonObj.type = TemplateButtonTypeModel.QUICK_REPLY;
                                buttonObj.text = button.Text.Trim();

                                buttons.buttons.Add(buttonObj);
                            }
                            else if (button.Type == TemplateButtonTypeModel.PHONE_NUMBER
                                && !String.IsNullOrWhiteSpace(button.Text)
                                && !String.IsNullOrWhiteSpace(button.PhoneNumber))
                            {
                                dynamic buttonObj = new ExpandoObject();

                                buttonObj.type = TemplateButtonTypeModel.PHONE_NUMBER;
                                buttonObj.text = button.Text.Trim();
                                buttonObj.phone_number = button.PhoneNumber.Trim();

                                buttons.buttons.Add(buttonObj);
                            }
                            else if (button.Type == TemplateButtonTypeModel.URL
                                && !String.IsNullOrWhiteSpace(button.Text)
                                && !String.IsNullOrWhiteSpace(button.Url))
                            {
                                dynamic buttonObj = new ExpandoObject();

                                buttonObj.type = TemplateButtonTypeModel.URL;
                                buttonObj.text = button.Text.Trim();
                                buttonObj.url = button.Url.Trim();

                                if (!String.IsNullOrWhiteSpace(button.Example))
                                {
                                    buttonObj.example = new List<string>
                                {
                                    button.Example.Trim()
                                };
                                }

                                buttons.buttons.Add(buttonObj);
                            }
                        }
                    }

                    if (model.Flow != null)
                    {
                        dynamic buttonObj = new ExpandoObject();

                        buttonObj.type = TemplateButtonTypeModel.FLOW;
                        buttonObj.text = model.Flow.ButtonText?.Trim();
                        buttonObj.flow_id = model.Flow.FlowId?.Trim();

                        buttons.buttons.Add(buttonObj);
                    }

                    template.components.Add(buttons);
                }

                requestStr = JsonConvert.SerializeObject(template);

                _logger.LogDebug("Created Message Template Request in function HandleMessageTemplateOps with received object={object} with request={request}", JsonConvert.SerializeObject(model), requestStr);

                // Send the update request   
                if (!String.IsNullOrWhiteSpace(messageTemplateId))
                {
                    var apiCallStart = DateTime.UtcNow;
                    resp = await _httpClient.PostAsync($"/{messageTemplateId}", new StringContent(requestStr, null, "application/json"));
                    responseStr = await resp.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received Update Template Message Response in function HandleMessageTemplateOps with received object={object} with request={request} and response={response} with apiResponseTime={apiResponseTime}", JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var updateTemplate = JsonConvert.DeserializeObject<UpdateMessageTemplateResponseModel>(responseStr);
                    if (updateTemplate != null)
                    {
                        if (updateTemplate.error != null)
                        {
                            StringBuilder err = new StringBuilder();
                            if (!String.IsNullOrWhiteSpace(updateTemplate.error.message))
                                err.Append(String.Concat(updateTemplate.error.message, ","));
                            if (!String.IsNullOrWhiteSpace(updateTemplate.error.error_user_title))
                                err.Append(String.Concat(updateTemplate.error.error_user_title, ","));
                            if (!String.IsNullOrWhiteSpace(updateTemplate.error.error_user_msg))
                                err.Append(String.Concat(updateTemplate.error.error_user_msg, ","));

                            return new ApiResult
                            {
                                StatusCode = 400,
                                Message = err.ToString().TrimEnd(',')
                            };
                        }
                        else if (updateTemplate.success) //Get template by id
                        {
                            var response = await _httpClient.GetAsync($"/{messageTemplateId}");
                            var content = await response.Content.ReadAsStringAsync();
                            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

                            return new ApiResult
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Template updated successfully",
                                Result = new
                                {
                                    Id = messageTemplate.id,
                                    Status = messageTemplate.status,
                                    Category = messageTemplate.category,
                                    Name = messageTemplate.name
                                }
                            };
                        }
                    }
                }
                else //Send the create request
                {
                    var apiCallStart = DateTime.UtcNow;
                    resp = await _httpClient.PostAsync($"/{senderNameInfo.BusinessAccountId}/message_templates", new StringContent(requestStr, null, "application/json"));
                    responseStr = await resp.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received Create Template Message Response in function HandleMessageTemplateOps with received object={object} with request={request} and response={response} with apiResponseTime={apiResponseTime}", JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var createTemplate = JsonConvert.DeserializeObject<CreateMessageTemplateResponseModel>(responseStr);
                    if (createTemplate != null)
                    {
                        if (createTemplate.error != null)
                        {
                            StringBuilder err = new StringBuilder();
                            if (!String.IsNullOrWhiteSpace(createTemplate.error.message))
                                err.Append(String.Concat(createTemplate.error.message, ","));
                            if (!String.IsNullOrWhiteSpace(createTemplate.error.error_user_title))
                                err.Append(String.Concat(createTemplate.error.error_user_title, ","));
                            if (!String.IsNullOrWhiteSpace(createTemplate.error.error_user_msg))
                                err.Append(String.Concat(createTemplate.error.error_user_msg, ","));

                            return new ApiResult
                            {
                                StatusCode = 400,
                                Message = err.ToString().TrimEnd(',')
                            };
                        }
                        else //Get template by id
                        {
                            var response = await _httpClient.GetAsync($"/{createTemplate.id}");
                            var content = await response.Content.ReadAsStringAsync();
                            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

                            return new ApiResult
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Template created successfully",
                                Result = new
                                {
                                    Id = messageTemplate.id,
                                    Status = messageTemplate.status,
                                    Category = messageTemplate.category,
                                    Name = messageTemplate.name
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleMessageTemplateOps with received object={object} with request={request} and response={response}", ex, JsonConvert.SerializeObject(model), requestStr, responseStr);
            }

            return new ApiResult
            {
                StatusCode = 400,
                Message = "Something went wrong"
            };
        }

        /// <summary>
        /// Send batch template messages
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> HandleSendBatchTemplateMessage(SendMessageTemplateRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {
                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Replace("+", "").Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogDebug("Calling function HandleSendBatchTemplateMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);

                var template = GetTemplateContent(model);

                var senderInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderInfo == null)
                {
                    return new ApiResult
                    {
                        StatusCode = 404,
                        Message = $"Client not found with clientId: {model.ClientId}"
                    };
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderInfo.AccessToken}");

                for (int i = 0; i < batches.Count; i++)
                {
                    string requestStr = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var batch = batches[i];
                        var batchRequest = new BatchMessageRequestModel
                        {
                            batch = batch.Select(recipient => new BatchMessageRequestModel.Batch
                            {
                                method = "POST",
                                relative_url = $"{senderInfo.PhoneNumberId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={template.type}&{template.type}={JsonConvert.SerializeObject(template.template)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogDebug("Created Batch Request in function HandleSendBatchTemplateMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        var apiCallStart = DateTime.UtcNow;

                        // Send the batch request   
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendBatchTemplateMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse} with apiResponseTime={apiResponseTime}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

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
                                    PhoneNumber = response.bodyResponse.contacts[0].wa_id,
                                    WAId = response.bodyResponse.messages[0].id,
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
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchTemplateMessage with received object={object} with batch size={batchSize} and batchCount={batchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchTemplateMessage with received object={object}", ex, JsonConvert.SerializeObject(model));
            }

            _logger.LogDebug("Execution ends for function HandleSendBatchTemplateMessage with received object={object} and response={response}", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(models));

            if (!models.Any())
            {
                return new ApiResult
                {
                    StatusCode = 400,
                    Message = "Couldn't send messages",
                    Result = models
                };
            }

            return new ApiResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Data processed succesfully",
                Result = models
            };
        }

        #endregion

        #region Media

        /// <summary>
        /// Handle media upload
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>
        public async Task<ApiResult> HandleMediaUpload(UploadMediaDto model)
        {
            _logger.LogDebug("Calling function HandleMediaUpload with received payload={payload}", JsonConvert.SerializeObject(model));

            var senderInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
            if (senderInfo == null)
            {
                return new ApiResult
                {
                    StatusCode = 404,
                    Message = $"Sender not found with clientId: {model.ClientId} and senderId: {model.SenderNameId}"
                };
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderInfo.AccessToken}");

            List<UploadMediaResultDto> results = new List<UploadMediaResultDto>();
            foreach (var item in model.Medias)
            {
                var media = await UploadMedia(senderInfo.PhoneNumberId, item);
                results.Add(media);
            }

            return new ApiResult
            {
                Success = true,
                Result = results,
                Message = "Media(s) processed successfully",
                StatusCode = 200
            };
        }

        #endregion

        #region Carousel

        /// <summary>
        /// Handle carousel template OPS
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>
        public async Task<ApiResult> HandleCarouselTemplateOps(CreateCarouselTemplateRequestDto model)
        {
            string requestStr = String.Empty;
            string responseStr = String.Empty;
            string fullUrl = String.Empty;

            try
            {
                //Replace empty spaces in template name with _
                model.Name = model.Name.Replace(" ", "_");

                _logger.LogDebug("Calling function HandleCarouselTemplateOps with received payload={payload}", JsonConvert.SerializeObject(model));

                var senderNameInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderNameInfo == null)
                {
                    return new ApiResult
                    {
                        StatusCode = 404,
                        Message = $"Sender name not found with clientId: {model.ClientId} and senderNameId: {model.SenderNameId}"
                    };
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderNameInfo.AccessToken}");

                string messageTemplateId = String.Empty;


                fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{senderNameInfo.BusinessAccountId}/message_templates?fields=name,id,status,language&name={model.Name}&language={model.LanguageCode}&limit=1");

                var resp = await _httpClient.GetAsync($"/{senderNameInfo.BusinessAccountId}/message_templates?fields=name,id,status,language&name={model.Name}&language={model.LanguageCode}&limit=1");
                responseStr = await resp.Content.ReadAsStringAsync();

                var messageTemplates = JsonConvert.DeserializeObject<MessageTemplateListResponse>(responseStr);
                if (messageTemplates != null && messageTemplates.data != null && messageTemplates.data.Any())
                {
                    var existingTemplate = messageTemplates.data[0];
                    if (existingTemplate.name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase)
                        && existingTemplate.language.Equals(model.LanguageCode, StringComparison.InvariantCultureIgnoreCase))
                        messageTemplateId = messageTemplates.data[0].id;
                }

                var template = new CreateMessageTemplateRequestModel
                {
                    name = model.Name.Replace(" ", "_").Trim(),
                    category = model.Category.Trim(),
                    language = model.LanguageCode.Trim(),
                    allow_category_change = false
                };

                //Add Body
                if (model.Body != null && !String.IsNullOrWhiteSpace(model.Body.Text))
                {
                    dynamic body = new ExpandoObject();

                    body.type = "BODY";
                    body.text = model.Body.Text.Trim();

                    if (model.Body.Examples.Any())
                    {
                        body.example = new
                        {
                            body_text = new List<object>
                            {
                                model.Body.Examples
                            }
                        };
                    }

                    template.components.Add(body);
                }

                if (model.Cards.Any())
                {
                    dynamic cardComponent = new ExpandoObject();
                    cardComponent.type = "carousel";
                    cardComponent.cards = new List<dynamic>();

                    foreach (var card in model.Cards)
                    {
                        dynamic cardObj = new ExpandoObject();
                        cardObj.components = new List<dynamic>();

                        //Add Header
                        if (card.Header != null && card.Header.Format != TemplateHeaderFormatTypeModel.NONE)
                        {
                            card.Header.Format = card.Header.Format.Trim().ToUpper();
                            dynamic header = new ExpandoObject();
                            header.type = "HEADER";
                            header.format = card.Header.Format;

                            if (card.Header.Format == TemplateHeaderFormatTypeModel.TEXT)
                            {
                                header.text = card.Header.Text;

                                if (!String.IsNullOrWhiteSpace(card.Header.Example))
                                {
                                    header.example = new
                                    {
                                        header_text = new List<string> { card.Header.Example.Trim() }
                                    };
                                }
                            }

                            if (card.Header.Format == TemplateHeaderFormatTypeModel.IMAGE
                                || card.Header.Format == TemplateHeaderFormatTypeModel.VIDEO
                                || card.Header.Format == TemplateHeaderFormatTypeModel.DOCUMENT)
                            {
                                var fileHandleId = await UploadMediaAsset(senderNameInfo.PhoneNumberId, senderNameInfo.AccessToken, senderNameInfo.AppId, card.Header.MediaUrl);

                                if (String.IsNullOrWhiteSpace(fileHandleId))
                                {
                                    return new ApiResult
                                    {
                                        StatusCode = 404,
                                        Message = $"Cannot uploaded provided header media with url {card.Header.MediaUrl}"
                                    };
                                }

                                header.example = new
                                {
                                    header_handle = new List<string>
                                    {
                                        fileHandleId
                                    }
                                };
                            }

                            cardObj.components.Add(header);
                        }

                        //Add Body
                        if (card.Body != null && !String.IsNullOrWhiteSpace(card.Body.Text))
                        {
                            dynamic body = new ExpandoObject();

                            body.type = "BODY";
                            body.text = card.Body.Text.Trim();

                            if (card.Body.Examples.Any())
                            {
                                body.example = new
                                {
                                    body_text = new List<object>
                                    {
                                        card.Body.Examples
                                    }
                                };
                            }

                            cardObj.components.Add(body);
                        }

                        //Add Buttons
                        if (card.Buttons.Any())
                        {
                            dynamic buttons = new ExpandoObject();
                            buttons.type = "BUTTONS";
                            buttons.buttons = new List<dynamic>();

                            foreach (var button in card.Buttons)
                            {
                                button.Type = button.Type.Trim().ToUpper();
                                if (button.Type == TemplateButtonTypeModel.QUICK_REPLY && !String.IsNullOrWhiteSpace(button.Text))
                                {
                                    dynamic buttonObj = new ExpandoObject();

                                    buttonObj.type = TemplateButtonTypeModel.QUICK_REPLY;
                                    buttonObj.text = button.Text.Trim();

                                    buttons.buttons.Add(buttonObj);
                                }
                                else if (button.Type == TemplateButtonTypeModel.PHONE_NUMBER
                                    && !String.IsNullOrWhiteSpace(button.Text)
                                    && !String.IsNullOrWhiteSpace(button.PhoneNumber))
                                {
                                    dynamic buttonObj = new ExpandoObject();

                                    buttonObj.type = TemplateButtonTypeModel.PHONE_NUMBER;
                                    buttonObj.text = button.Text.Trim();
                                    buttonObj.phone_number = button.PhoneNumber.Trim();

                                    buttons.buttons.Add(buttonObj);
                                }
                                else if (button.Type == TemplateButtonTypeModel.URL
                                    && !String.IsNullOrWhiteSpace(button.Text)
                                    && !String.IsNullOrWhiteSpace(button.Url))
                                {
                                    dynamic buttonObj = new ExpandoObject();

                                    buttonObj.type = TemplateButtonTypeModel.URL;
                                    buttonObj.text = button.Text.Trim();
                                    buttonObj.url = button.Url.Trim();

                                    if (!String.IsNullOrWhiteSpace(button.Example))
                                    {
                                        buttonObj.example = new List<string>
                                        {
                                            button.Example.Trim()
                                        };
                                    }

                                    buttons.buttons.Add(buttonObj);
                                }
                            }

                            cardObj.components.Add(buttons);
                        }

                        cardComponent.cards.Add(cardObj);
                    }

                    template.components.Add(cardComponent);
                }

                requestStr = JsonConvert.SerializeObject(template);

                _logger.LogDebug("Created Message Template Request in function HandleCarouselTemplateOps with received object={object} with request={request}", JsonConvert.SerializeObject(model), requestStr);

                // Send the update request   
                if (!String.IsNullOrWhiteSpace(messageTemplateId))
                {
                    var apiCallStart = DateTime.UtcNow;
                    var endpoint = $"/{messageTemplateId}";
                    resp = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));
                    responseStr = await resp.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received Update Template Message Response in function HandleCarouselTemplateOps with received object={object} with apiEndpoint={apiEndpoint} with request={request} and response={response} with apiResponseTime={apiResponseTime}", endpoint, JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var updateTemplate = JsonConvert.DeserializeObject<UpdateMessageTemplateResponseModel>(responseStr);
                    if (updateTemplate != null)
                    {
                        if (updateTemplate.error != null)
                        {
                            StringBuilder err = new StringBuilder();
                            if (!String.IsNullOrWhiteSpace(updateTemplate.error.message))
                                err.Append(String.Concat(updateTemplate.error.message, ","));
                            if (!String.IsNullOrWhiteSpace(updateTemplate.error.error_user_title))
                                err.Append(String.Concat(updateTemplate.error.error_user_title, ","));
                            if (!String.IsNullOrWhiteSpace(updateTemplate.error.error_user_msg))
                                err.Append(String.Concat(updateTemplate.error.error_user_msg, ","));

                            return new ApiResult
                            {
                                StatusCode = 400,
                                Message = err.ToString().TrimEnd(',')
                            };
                        }
                        else if (updateTemplate.success) //Get template by id
                        {
                            var response = await _httpClient.GetAsync($"/{messageTemplateId}");
                            var content = await response.Content.ReadAsStringAsync();
                            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

                            return new ApiResult
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Template updated successfully",
                                Result = new
                                {
                                    Id = messageTemplate.id,
                                    Status = messageTemplate.status,
                                    Category = messageTemplate.category,
                                    Name = messageTemplate.name
                                }
                            };
                        }
                    }
                }
                else //Send the create request
                {

                    var apiCallStart = DateTime.UtcNow;
                    var endpoint = $"/{senderNameInfo.BusinessAccountId}/message_templates";
                    resp = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));
                    responseStr = await resp.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received Create Template Message Response in function HandleCarouselTemplateOps with received object={object} with apiEndpoint={apiEndpoint} with request={request} and response={response} with apiResponseTime={apiResponseTime}", endpoint, JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var createTemplate = JsonConvert.DeserializeObject<CreateMessageTemplateResponseModel>(responseStr);
                    if (createTemplate != null)
                    {
                        if (createTemplate.error != null)
                        {
                            StringBuilder err = new StringBuilder();
                            if (!String.IsNullOrWhiteSpace(createTemplate.error.message))
                                err.Append(String.Concat(createTemplate.error.message, ","));
                            if (!String.IsNullOrWhiteSpace(createTemplate.error.error_user_title))
                                err.Append(String.Concat(createTemplate.error.error_user_title, ","));
                            if (!String.IsNullOrWhiteSpace(createTemplate.error.error_user_msg))
                                err.Append(String.Concat(createTemplate.error.error_user_msg, ","));

                            return new ApiResult
                            {
                                StatusCode = 400,
                                Message = err.ToString().TrimEnd(',')
                            };
                        }
                        else //Get template by id
                        {
                            var response = await _httpClient.GetAsync($"/{createTemplate.id}");
                            var content = await response.Content.ReadAsStringAsync();
                            var messageTemplate = JsonConvert.DeserializeObject<MessageTemplateModel>(await response.Content.ReadAsStringAsync());

                            return new ApiResult
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Template created successfully",
                                Result = new
                                {
                                    Id = messageTemplate.id,
                                    Status = messageTemplate.status,
                                    Category = messageTemplate.category,
                                    Name = messageTemplate.name
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleCarouselTemplateOps with received object={object} with request={request} and response={response}", ex, JsonConvert.SerializeObject(model), requestStr, responseStr);
            }

            return new ApiResult
            {
                StatusCode = 400,
                Message = "Something went wrong"
            };
        }

        /// <summary>
        /// Send batch carousel messages
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> HandleSendBatchCarouselMessage(SendMessageCarouselRequestDto model)
        {
            List<SendMessageResponseDto> models = new List<SendMessageResponseDto>();

            try
            {
                model.PhoneNumbers = model.PhoneNumbers.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Replace("+", "").Trim()).ToList();
                int batchSize = _whatsAppConfigurationSetting.Value.SendMessageBatchSize;
                var batches = model.PhoneNumbers.ChunkBy(batchSize);

                _logger.LogDebug("Calling function HandleSendBatchCarouselMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount}", JsonConvert.SerializeObject(model), batchSize, batches.Count);

                var template = GetCarouselTemplateContent(model);

                var senderInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderInfo == null)
                {
                    return new ApiResult
                    {
                        StatusCode = 404,
                        Message = $"Client not found with clientId: {model.ClientId}"
                    };
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderInfo.AccessToken}");

                for (int i = 0; i < batches.Count; i++)
                {
                    string requestStr = String.Empty;
                    string responseStr = String.Empty;

                    try
                    {
                        var batch = batches[i];
                        var batchRequest = new BatchMessageRequestModel
                        {
                            batch = batch.Select(recipient => new BatchMessageRequestModel.Batch
                            {
                                method = "POST",
                                relative_url = $"{senderInfo.PhoneNumberId}/messages",
                                body = $"messaging_product=whatsapp&recipient_type=individual&to={recipient}&type={template.type}&{template.type}={JsonConvert.SerializeObject(template.template)}"
                            }).ToList()
                        };

                        requestStr = JsonConvert.SerializeObject(batchRequest);

                        _logger.LogDebug("Created Batch Request in function HandleSendBatchCarouselMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr);

                        var apiCallStart = DateTime.UtcNow;

                        // Send the batch request   
                        var resp = await _httpClient.PostAsync("", new StringContent(requestStr, null, "application/json"));
                        responseStr = await resp.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Batch Response in function HandleSendBatchCarouselMessage with received object={object} with batch size={batchSize} and totalbatchCount={totalbatchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse} with apiResponseTime={apiResponseTime}", JsonConvert.SerializeObject(model), batchSize, batches.Count, (i + 1), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

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
                                    PhoneNumber = response.bodyResponse.contacts[0].wa_id,
                                    WAId = response.bodyResponse.messages[0].id,
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
                        _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchCarouselMessage with received object={object} with batch size={batchSize} and batchCount={batchCount} and batchIndex={batchIndex} and batchRequest={batchRequest} and batchResponse={batchResponse}", ex, JsonConvert.SerializeObject(model), batchSize, batches.Count, i, requestStr, responseStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleSendBatchCarouselMessage with received object={object}", ex, JsonConvert.SerializeObject(model));
            }

            _logger.LogDebug("Execution ends for function HandleSendBatchCarouselMessage with received object={object} and response={response}", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(models));

            if (!models.Any())
            {
                return new ApiResult
                {
                    StatusCode = 400,
                    Message = "Couldn't send messages",
                    Result = models
                };
            }

            return new ApiResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Data processed succesfully",
                Result = models
            };
        }

        #endregion

        #region Flows

        /// <summary>
        /// Handle flow OPS
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> HandleFlowOps(CreateFlowRequestDto model)
        {
            string requestStr = String.Empty;
            string responseStr = String.Empty;
            string filepath = String.Empty;

            try
            {
                //Replace empty spaces in flow name with _
                model.Name = (model.Name ?? "").Replace(" ", "_").ToLower().Trim();
                model.Category = (model.Category ?? "").Replace(" ", "_").ToLower().Trim();
                model.EndpointUrl = (model.EndpointUrl ?? "").ToLower().Trim();

                _logger.LogDebug("Calling function HandleFlowOps with received object={object}", JsonConvert.SerializeObject(model));

                var senderNameInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderNameInfo == null)
                    return new ApiResult { StatusCode = 404, Message = $"Sender name not found with clientId: {model.ClientId} and senderNameId: {model.SenderNameId}" };

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderNameInfo.AccessToken}");

                // Send the update flow request   
                if (!String.IsNullOrWhiteSpace(model.FlowId))
                {
                    //Flow Path
                    var flowDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "Flow");
                    if (!Directory.Exists(flowDirectory))
                        Directory.CreateDirectory(flowDirectory);

                    // Get the file name from the Uri
                    string filename = String.Concat(Guid.NewGuid().ToString(), ".json"); // Path.GetFileName(model.File.FileName);
                    filepath = Path.Combine(flowDirectory, filename);

                    await using var savefileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                    byte[] data = Encoding.UTF8.GetBytes(model.FlowJson);
                    savefileStream.Write(data, 0, data.Length);

                    savefileStream.Close();
                    savefileStream.Dispose();

                    var endpoint = $"{model.FlowId}/assets";
                    var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

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

                        // Add file content to the form-data
                        content.Add(fileContent, "file", Path.GetFileName(filepath));

                        // Add other form data parameters   
                        content.Add(new StringContent("flow.json"), "name");
                        content.Add(new StringContent("FLOW_JSON"), "asset_type");

                        // Assign content to the request
                        request.Content = content;

                        var apiCallStart = DateTime.UtcNow;

                        // Send the request and get the response
                        var response = await _httpClient.SendAsync(request);

                        // Read the response content
                        responseStr = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation("Received Update Flow Response in function HandleFlowOps with apiEndpoint={apiEndpoint} with received object={object} with response={response} with apiResponseTime={apiResponseTime}", endpoint, JsonConvert.SerializeObject(model), responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                        var updateFlow = JsonConvert.DeserializeObject<FlowAssetUploadResponse>(responseStr);
                        if (updateFlow != null)
                        {
                            if (updateFlow.error != null)
                            {
                                StringBuilder err = new StringBuilder();
                                if (!String.IsNullOrWhiteSpace(updateFlow.error.message))
                                    err.Append(String.Concat(updateFlow.error.message, ","));
                                if (!String.IsNullOrWhiteSpace(updateFlow.error.error_user_title))
                                    err.Append(String.Concat(updateFlow.error.error_user_title, ","));
                                if (!String.IsNullOrWhiteSpace(updateFlow.error.error_user_msg))
                                    err.Append(String.Concat(updateFlow.error.error_user_msg, ","));

                                return new ApiResult
                                {
                                    StatusCode = 400,
                                    Message = err.ToString().TrimEnd(',')
                                };
                            }
                            else if (updateFlow.validation_errors != null && updateFlow.validation_errors.Count > 0)
                            {
                                return new ApiResult
                                {
                                    StatusCode = 400,
                                    Message = "Cannot update flow",
                                    Result = new
                                    {
                                        Id = model.FlowId,
                                        ValidationErrors = updateFlow.validation_errors
                                    }
                                };
                            }
                            else //Get flow by id
                            {
                                response = await _httpClient.GetAsync($"/{model.FlowId}");
                                var resp = await response.Content.ReadAsStringAsync();
                                var flow = JsonConvert.DeserializeObject<FlowByIdResponse>(resp);

                                return new ApiResult
                                {
                                    Success = true,
                                    StatusCode = 200,
                                    Message = "Flow updated successfully",
                                    Result = new
                                    {
                                        Id = flow.id,
                                        Name = flow.name,
                                        Status = flow.status, //DRAFT AND PUBLISHED
                                        categories = flow.categories,
                                        ValidationErrors = flow.validation_errors
                                    }
                                };
                            }
                        }
                    }
                }
                else //Send the create flow request
                {
                    var flowRequest = new CreateFlowRequestModel
                    {
                        name = model.Name,
                        categories = new List<string> { model.Category },
                        endpoint_uri = model.EndpointUrl,
                        flow_json = model.FlowJson
                    };

                    requestStr = JsonConvert.SerializeObject(flowRequest);

                    var apiCallStart = DateTime.UtcNow;
                    var endpoint = $"/{senderNameInfo.BusinessAccountId}/flows";
                    var resp = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));
                    responseStr = await resp.Content.ReadAsStringAsync();

                    _logger.LogInformation("Received Create Flow Response in function HandleFlowOps with apiEndpoint={apiEndpoint} with received object={object} with request={request} and response={response} with apiResponseTime={apiResponseTime}", endpoint, JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                    var createFlow = JsonConvert.DeserializeObject<CreateFlowResponseModel>(responseStr);
                    if (createFlow != null)
                    {
                        if (createFlow.error != null)
                        {
                            StringBuilder err = new StringBuilder();
                            if (!String.IsNullOrWhiteSpace(createFlow.error.message))
                                err.Append(String.Concat(createFlow.error.message, ","));
                            if (!String.IsNullOrWhiteSpace(createFlow.error.error_user_title))
                                err.Append(String.Concat(createFlow.error.error_user_title, ","));
                            if (!String.IsNullOrWhiteSpace(createFlow.error.error_user_msg))
                                err.Append(String.Concat(createFlow.error.error_user_msg, ","));

                            return new ApiResult
                            {
                                StatusCode = 400,
                                Message = err.ToString().TrimEnd(',')
                            };
                        }
                        else //Get flow by id
                        {
                            var response = await _httpClient.GetAsync($"/{createFlow.id}");
                            var content = await response.Content.ReadAsStringAsync();
                            var flow = JsonConvert.DeserializeObject<FlowByIdResponse>(content);

                            return new ApiResult
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Flow created successfully",
                                Result = new
                                {
                                    Id = flow.id,
                                    Name = flow.name,
                                    Status = flow.status,
                                    categories = flow.categories,
                                    ValidationErrors = flow.validation_errors
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleFlowOps with received object={object} with request={request} and response={response}", ex, JsonConvert.SerializeObject(model), requestStr, responseStr);
            }
            finally //Delete the saved flow JSON file
            {
                if (!String.IsNullOrWhiteSpace(filepath) && File.Exists(filepath))
                    File.Delete(filepath);
            }

            return new ApiResult
            {
                StatusCode = 400,
                Message = "Something went wrong"
            };
        }

        /// <summary>
        /// Handle publish flow
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> HandlePublishFlow(PublishFlowRequestDto model)
        {
            string requestStr = String.Empty;
            string responseStr = String.Empty;

            try
            {
                _logger.LogDebug("Calling function HandlePublishFlow with received object={object}", JsonConvert.SerializeObject(model));

                var senderNameInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderNameId);
                if (senderNameInfo == null)
                    return new ApiResult { StatusCode = 404, Message = $"Sender name not found with clientId: {model.ClientId} and senderNameId: {model.SenderNameId}" };

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderNameInfo.AccessToken}");

                var apiCallStart = DateTime.UtcNow;
                var endpoint = $"/{model.FlowId}/publish";
                var resp = await _httpClient.PostAsync(endpoint, new StringContent(requestStr, null, "application/json"));
                responseStr = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("Received Publish Flow Response in function HandlePublishFlow with apiEndpoint={apiEndpoint} with received object={object} with request={request} and response={response} with apiResponseTime={apiResponseTime}", endpoint, JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                var publishFlow = JsonConvert.DeserializeObject<PublishFlowResponseModel>(responseStr);
                if (publishFlow != null)
                {
                    if (publishFlow.error != null)
                    {
                        StringBuilder err = new StringBuilder();
                        if (!String.IsNullOrWhiteSpace(publishFlow.error.message))
                            err.Append(String.Concat(publishFlow.error.message, ","));
                        if (!String.IsNullOrWhiteSpace(publishFlow.error.error_user_title))
                            err.Append(String.Concat(publishFlow.error.error_user_title, ","));
                        if (!String.IsNullOrWhiteSpace(publishFlow.error.error_user_msg))
                            err.Append(String.Concat(publishFlow.error.error_user_msg, ","));

                        return new ApiResult
                        {
                            StatusCode = 400,
                            Message = err.ToString().TrimEnd(',')
                        };
                    }
                    else if (publishFlow.validation_errors != null && publishFlow.validation_errors.Count > 0)
                    {
                        return new ApiResult
                        {
                            StatusCode = 400,
                            Message = "Cannot publish flow",
                            Result = new
                            {
                                Id = model.FlowId,
                                ValidationErrors = publishFlow.validation_errors
                            }
                        };
                    }
                    else //Get flow by id
                    {
                        var response = await _httpClient.GetAsync($"/{model.FlowId}");
                        var content = await response.Content.ReadAsStringAsync();
                        var flow = JsonConvert.DeserializeObject<FlowByIdResponse>(content);

                        return new ApiResult
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = "Flow published successfully",
                            Result = new
                            {
                                Id = flow.id,
                                Name = flow.name,
                                Status = flow.status,
                                categories = flow.categories,
                                ValidationErrors = flow.validation_errors
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandlePublishFlow with received object={object} with request={request} and response={response}", ex, JsonConvert.SerializeObject(model), requestStr, responseStr);
            }

            return new ApiResult
            {
                StatusCode = 400,
                Message = "Something went wrong"
            };
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Fetch meta analytics 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResult> FetchConversationAnalytics(ConversationAnalyticsRequestDto model)
        {
            string requestStr = String.Empty;
            string responseStr = String.Empty;
            string filepath = String.Empty;
            List<ConversationAnalyticsResponseDto> response = new List<ConversationAnalyticsResponseDto>();

            try
            {
                _logger.LogDebug("Calling function FetchConversationAnalytics with received object={object}", JsonConvert.SerializeObject(model));

                var senderNameInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderId);
                if (senderNameInfo == null)
                    return new ApiResult { StatusCode = 404, Message = $"Sender name not found with clientId: {model.ClientId} and senderNameId: {model.SenderId}" };

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderNameInfo.AccessToken}");

                long startEpoch = CommonHelper.ConvertToEpoch(model.StartDate);
                long endEpoch = CommonHelper.ConvertToEpoch(model.EndDate);

                var apiCallStart = DateTime.UtcNow;
                var endpoint = $"/{senderNameInfo.BusinessAccountId}?fields=conversation_analytics.start({startEpoch}).end({endEpoch}).phonenumber({senderNameInfo.PhoneNumber}).granularity(DAILY).dimensions([\"CONVERSATION_CATEGORY\",\"CONVERSATION_TYPE\",\"PHONE\"])";
                var resp = await _httpClient.GetAsync(endpoint);
                responseStr = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("Received Conversation Analytics Response in function FetchConversationAnalytics with apiEndpoint={apiEndpoint} with received object={object} with request={request} and response={response} with apiResponseTime={apiResponseTime}", endpoint, JsonConvert.SerializeObject(model), requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                var analyticsResponse = JsonConvert.DeserializeObject<ConversationAnalyticsResponseModel>(responseStr);

                if (analyticsResponse != null
                    && analyticsResponse.conversation_analytics != null
                    && analyticsResponse.conversation_analytics.data != null
                    && analyticsResponse.conversation_analytics.data.Count > 0)
                {
                    var dataList = analyticsResponse.conversation_analytics.data[0].data_points;
                    foreach (var data in dataList)
                    {
                        var dataPoint = new ConversationAnalyticsResponseDto
                        {
                            ClientId = model.ClientId,
                            SenderId = model.SenderId,
                            Conversation = data.conversation,
                            ConversationCategory = data.conversation_category,
                            ConversationType = data.conversation_type,
                            Cost = data.cost,
                            PhoneNumber = data.phone_number,
                            Start = data.start,
                            End = data.end,
                            StartDateUtc = data.StartDateUtc,
                            EndDateUtc = data.EndDateUtc
                        };

                        response.Add(dataPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function FetchConversationAnalytics with received object={object} with request={request} and response={response}", ex, JsonConvert.SerializeObject(model), requestStr, responseStr);
                response = null;
            }

            if (response == null || !response.Any())
            {
                return new ApiResult
                {
                    StatusCode = 400,
                    Message = "Couldn't fetch conversation analytics"
                };
            }

            return new ApiResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Success",
                Result = response
            };
        }



        public async Task<ApiResult> FetchTemplateAnalytics(TemplateAnalyticsRequestDto model)
        {
            string requestStr = string.Empty;
            string responseStr = string.Empty;

            try
            {
                _logger.LogDebug("Calling function FetchTemplateAnalytics with received object={object}", JsonConvert.SerializeObject(model));

                var senderNameInfo = await _integrationHandler.GetSenderInformation(model.ClientId, model.SenderId);
                if (senderNameInfo == null)
                {
                    return new ApiResult
                    {
                        StatusCode = 404,
                        Message = $"Sender name not found with clientId: {model.ClientId} and senderNameId: {model.SenderId}"
                    };
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {senderNameInfo.AccessToken}");

                long startEpoch = CommonHelper.ConvertToEpoch(model.StartDate);
                long endEpoch = CommonHelper.ConvertToEpoch(model.EndDate);

                var apiCallStart = DateTime.UtcNow;
                var endpoint = $"/{senderNameInfo.BusinessAccountId}/template_analytics?" +
                               $"start={startEpoch}&" +
                               $"end={endEpoch}&" +
                               $"granularity=daily&" +
                               $"metric_types=cost%2Cclicked%2Cdelivered%2Cread%2Csent&" +
                               $"template_ids=[{model.TemplateId}]";

                var resp = await _httpClient.GetAsync(endpoint);
                responseStr = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "Received Template Analytics Response in function FetchTemplateAnalytics | endpoint={endpoint} | request={request} | response={response} | responseTime={responseTime}ms",
                    endpoint, requestStr, responseStr, DateTime.UtcNow.Subtract(apiCallStart).TotalMilliseconds);

                if (!resp.IsSuccessStatusCode)
                {
                    return new ApiResult
                    {
                        StatusCode = (int)resp.StatusCode,
                        Message = $"Failed to fetch data. Status: {(int)resp.StatusCode}",
                        Result = responseStr
                    };
                }
                var analyticsResponse = JsonConvert.DeserializeObject<TemplateAnalyticsResponseDto>(responseStr);

                return new ApiResult
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Success",
                    Result = analyticsResponse.Data[0]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in FetchTemplateAnalytics: {exception} | input={input} | response={response}",
                    ex, JsonConvert.SerializeObject(model), responseStr);

                return new ApiResult
                {
                    StatusCode = 500,
                    Message = "Exception occurred while fetching template analytics.",
                    Result = ex.Message
                };
            }
        }

        #endregion

        #endregion
    }
}
