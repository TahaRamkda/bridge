namespace WhatsAppBridge.Models.WhatsApp
{
    public class MessageTemplateListResponse
    {
        public MessageTemplateListResponse()
        {
            data = new List<MessageTemplate>();
        }

        public List<MessageTemplate> data { get; set; }

        public class MessageTemplate
        {
            public string id { get; set; }
            public string name { get; set; }
            public string status { get; set; }
        }
    }
}
