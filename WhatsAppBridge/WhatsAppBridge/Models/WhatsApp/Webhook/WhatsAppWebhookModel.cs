using Newtonsoft.Json;

namespace WhatsAppBridge.Models.WhatsApp.Webhook
{
    public partial class WhatsAppWebhookModel
    {
        public WhatsAppWebhookModel()
        {
            entry = new List<Entry>();
        }

        [JsonProperty("object")]
        public string Object { get; set; }

        public List<Entry> entry { get; set; }

        public partial class Entry
        {
            public Entry()
            {
                changes = new List<Change>();
            }

            public string id { get; set; }
            public string time { get; set; }
            public List<Change> changes { get; set; }
        }

        public partial class Change
        {
            public string field { get; set; }
            public object  value { get; set; }
        }
    }
}
