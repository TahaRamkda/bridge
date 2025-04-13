namespace WhatsAppBridge.Models.Integration
{
    public class SendMessageCarouselRequestDto
    {
        public SendMessageCarouselRequestDto()
        {
            PhoneNumbers = new List<string>();
            BodyParams = new List<string>();
        }

        public string ClientId { get; set; }
        public string SenderNameId { get; set; }
        public List<string> PhoneNumbers { get; set; }
        public string LanguageCode { get; set; }
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public List<string> PhoneNumbersList { get; set; }
        public List<string> BodyParams { get; set; }
        public List<CardComponent> Cards { get; set; }

        public class CardComponent
        {
            public CardComponent()
            {
            }

            public int Index { get; set; }
            public List<Component> Components { get; set; }
        }

        public class Component
        {
            public Component()
            {
                Values = new List<KeyValue>();
            }

            public string ComponentType { get; set; }
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
