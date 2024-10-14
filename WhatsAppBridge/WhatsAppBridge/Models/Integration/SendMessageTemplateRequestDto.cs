namespace WhatsAppBridge.Models.Integration
{
    public partial class SendMessageTemplateRequestDto
    {
        public SendMessageTemplateRequestDto()
        {
        }

        public string PhoneId { get; set; }
        public string PhoneNumber { get; set; }
        public string LanguageCode { get; set; }
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public List<Component> Components { get; set; }

        public class Component
        {
            public Component()
            {
                Values = new List<KeyValue>();
            }

            public string ComponentType { get; set; }
            //public string ComponentSubType { get; set; }
            public List<KeyValue> Values { get; set; }
        }

        public class KeyValue
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public int Index { get; set; }
        }
    }
}
