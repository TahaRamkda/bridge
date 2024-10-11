using Newtonsoft.Json;

namespace WhatsAppBridge.Models.WhatsApp.Webhook
{
    public partial class TemplateUpdateWebhook
    {
        [JsonProperty("event")]
        public string Event { get; set; }
        public string message_template_id { get; set; }
        public string message_template_name { get; set; }
        public string message_template_language { get; set; }
        public string reason { get; set; }
    }
}
