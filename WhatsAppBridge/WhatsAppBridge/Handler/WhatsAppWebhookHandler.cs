using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json;
using WhatsAppBridge.Controllers;
using WhatsAppBridge.Helpers;
using WhatsAppBridge.Models;
using WhatsAppBridge.Models.Integration;
using WhatsAppBridge.Models.Integration.Flow;
using WhatsAppBridge.Models.WhatsApp;
using WhatsAppBridge.Models.WhatsApp.Webhook;
using WhatsAppBridge.Settings;
using static WhatsAppBridge.Models.WhatsApp.Webhook.MessageUpdateWebhookModel;
using static WhatsAppBridge.Models.WhatsApp.Webhook.WhatsAppWebhookModel;

namespace WhatsAppBridge.Handler
{
    public partial class WhatsAppWebhookHandler
    {
        #region Fields

        private readonly ILogger<WhatsAppWebhookHandler> _logger;
        private readonly IOptions<WhatsAppConfigurationSetting> _whatsAppConfigurationSetting;
        private readonly IntegrationHandler _integrationHandler;
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = String.Empty;

        #endregion

        #region Ctor

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

        #endregion

        #region Utilities

        public FlowResponse GenerateFlowResponse(MessageUpdateWebhookModel.Message.Interactive.NFMReply reply)
        {
            var responseJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(reply.response_json);
            Dictionary<string, string> questionMapping = new();  // Stores Questions
            FlowResponse flowResponse = new FlowResponse();  // Stores Answers

            // Step 1: Extract flow_token if present
            if (responseJson.TryGetValue("flow_token", out var flowTokenValue) && flowTokenValue is JsonElement flowTokenElement && flowTokenElement.ValueKind == JsonValueKind.String)
                flowResponse.FlowToken = (flowTokenElement.GetString() ?? "").Replace(" ", "").Trim();

            // Step 2: Store all questions (keys ending in "_Q")
            foreach (var item in responseJson)
            {
                if (item.Key.EndsWith("_Q") && item.Value is JsonElement questionElement && questionElement.ValueKind == JsonValueKind.String)
                {
                    string baseKey = item.Key.Replace("_Q", ""); // Remove "_Q" for matching
                    questionMapping[baseKey] = questionElement.GetString();
                }
            }

            // Step 3: Match responses to questions and store question and answer keys
            foreach (var questionKey in questionMapping.Keys)
            {
                string type = "Text"; // Default type
                string textResponse = String.Empty;
                List<string> checkboxResponse = new List<string>();

                if (responseJson.TryGetValue(questionKey, out var responseValue) && responseValue is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.String) // Single response
                    {
                        textResponse = jsonElement.GetString();
                        type = "Text";
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.Array) // Checkbox response (array)
                    {
                        checkboxResponse = new List<string>();
                        foreach (var element in jsonElement.EnumerateArray())
                        {
                            checkboxResponse.Add(element.GetString());
                        }
                        type = "MultiSelect";
                    }
                }

                // Add to structured response
                flowResponse.Responses.Add(new FlowResponse.Response
                {
                    QuestionKey = questionKey + "_Q", // Store the question key (e.g., "Screen_One_C1_Q")
                    AnswerKey = questionKey, // Store the answer key (e.g., "Screen_One_C1")
                    Question = questionMapping[questionKey],
                    Type = type,
                    TextResponse = textResponse,
                    CheckboxResponse = checkboxResponse
                });
            }

            return flowResponse;
        }

        #endregion

        #region Methods

        public async Task HandleMessageTemplateStatusUpdate(string clientId, Change change)
        {
            try
            {
                _logger.LogInformation("Calling function HandleMessageTemplateStatusUpdate with received object {object}", JsonConvert.SerializeObject(change));

                TemplateUpdateWebhookModel templateUpdate = JsonConvert.DeserializeObject<TemplateUpdateWebhookModel>(JsonConvert.SerializeObject(change.value));

                var fullUrl = CommonHelper.GetFullUrl(baseUrl, $"/{templateUpdate.message_template_id}");
                _logger.LogInformation("Calling WhatsApp HandleMessageTemplateStatusUpdate method with templateId {templateId} with url {url}", templateUpdate.message_template_id, fullUrl);

                var clientInfo = await _integrationHandler.GetClientInformation(clientId);
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {clientInfo.AccessToken}");
                var response = await _httpClient.GetAsync($"/{templateUpdate.message_template_id}");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received response of WhatsApp HandleMessageTemplateStatusUpdate method with templateId {templateId} with url {url} and content {content}", templateUpdate.message_template_id, fullUrl, content);

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
                            update_dateTime = CommonHelper.ConvertDateTimeFormat(CommonHelper.ConvertFromEpoch(status.timestamp)),
                            recipient_Id = status.recipient_id
                        };

                        if (messageUpdate.metadata != null)
                        {
                            updateDto.phone_number_Id = new MessageStatusUpdateDto.PhoneNumber
                            {
                                display_phone_number = messageUpdate.metadata.display_phone_number,
                                phone_number_id = messageUpdate.metadata.phone_number_id
                            };
                        }

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

                if (messageUpdate.messages != null && messageUpdate.messages.Any())
                {
                    foreach (var message in messageUpdate.messages)
                    {
                        //Don't send sms update in case of system type, unsupported
                        if (message.type == "system" || message.type == "unsupported")
                            continue;

                        var updateDto = new MessageReceiveDto
                        {
                            client_Id = clientId,
                            wam_Id = message.id,
                            from = message.from,
                            update_dateTime = CommonHelper.ConvertDateTimeFormat(CommonHelper.ConvertFromEpoch(message.timestamp)),
                            type = message.type
                        };

                        if (messageUpdate.metadata != null)
                        {
                            updateDto.phone_number_Id = new MessageReceiveDto.PhoneNumber
                            {
                                display_phone_number = messageUpdate.metadata.display_phone_number,
                                phone_number_id = messageUpdate.metadata.phone_number_id
                            };
                        }

                        if (messageUpdate.contacts != null && messageUpdate.contacts.Any())
                        {
                            updateDto.contact = new MessageReceiveDto.Contact
                            {
                                wa_id = messageUpdate.contacts[0].wa_id,
                                name = messageUpdate.contacts[0].profile?.name ?? ""
                            };
                        }

                        if (message.context != null)
                        {
                            updateDto.context = new MessageReceiveDto.Context
                            {
                                from = message.context.from,
                                wam_Id = message.context.id,
                            };
                        }

                        if (message.text != null)
                        {
                            updateDto.text = new MessageReceiveDto.Text
                            {
                                body = message.text.body
                            };
                        }

                        if (message.button != null)
                        {
                            updateDto.button = new MessageReceiveDto.Button
                            {
                                text = message.button.text,
                                payload = message.button.payload
                            };
                        }

                        if (message.image != null)
                        {
                            updateDto.image = new MessageReceiveDto.Image
                            {

                                id = message.image.id,
                                caption = message.image.caption,
                                mime_type = message.image.mime_type,
                                sha256 = message.image.sha256
                            };
                        }

                        if (message.video != null)
                        {
                            updateDto.video = new MessageReceiveDto.Video
                            {
                                id = message.video.id,
                                caption = message.video.caption,
                                mime_type = message.video.mime_type,
                                sha256 = message.video.sha256
                            };
                        }

                        if (message.audio != null)
                        {
                            updateDto.audio = new MessageReceiveDto.Audio
                            {
                                id = message.audio.id,
                                voice = message.audio.voice,
                                mime_type = message.audio.mime_type,
                                sha256 = message.audio.sha256
                            };
                        }

                        if (message.document != null)
                        {
                            updateDto.document = new MessageReceiveDto.Document
                            {
                                id = message.document.id,
                                caption = message.document.caption,
                                mime_type = message.document.mime_type,
                                sha256 = message.document.sha256,
                                filename = message.document.filename
                            };
                        }

                        if (message.location != null)
                        {
                            updateDto.location = new MessageReceiveDto.Location
                            {
                                latitude = message.location.latitude,
                                longitude = message.location.longitude
                            };
                        }

                        if (message.sticker != null)
                        {
                            updateDto.sticker = new MessageReceiveDto.Sticker
                            {
                                id = message.sticker.id,
                                mime_type = message.sticker.mime_type,
                                sha256 = message.sticker.sha256,
                                animated = message.sticker.animated,
                                caption = message.sticker.caption
                            };
                        }

                        if (message.reaction != null)
                        {
                            updateDto.reaction = new MessageReceiveDto.Reaction
                            {
                                message_id = message.reaction.message_id,
                                emoji = message.reaction.emoji
                            };

                            if (message.context == null && updateDto.contact != null)
                            {
                                updateDto.context = new MessageReceiveDto.Context
                                {
                                    from = updateDto.contact.wa_id,
                                    wam_Id = message.reaction.message_id
                                };
                            }
                        }

                        if (message.interactive != null)
                        {
                            if (message.interactive.list_reply != null)
                            {
                                updateDto.listReply = new MessageReceiveDto.ListReply
                                {
                                    id = message.interactive.list_reply.id,
                                    title = message.interactive.list_reply.title
                                };
                            }

                            if (message.interactive.button_reply != null)
                            {
                                updateDto.buttonReply = new MessageReceiveDto.ButtonReply
                                {
                                    id = message.interactive.button_reply.id,
                                    title = message.interactive.button_reply.title
                                };
                            }

                            //If flow message received
                            if (message.interactive.nfm_reply != null)
                                updateDto.flowResponse = GenerateFlowResponse(message.interactive.nfm_reply);
                        }

                        await _integrationHandler.MessageReceiveUpdate(updateDto);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred {exception} when executing function HandleMessageStatusUpdate with received clientId {clientId} and object {object}", ex, clientId, JsonConvert.SerializeObject(change));
            }
        }

        #endregion
    }
}
