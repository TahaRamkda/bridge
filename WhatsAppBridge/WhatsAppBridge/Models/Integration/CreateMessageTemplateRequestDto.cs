namespace WhatsAppBridge.Models.Integration
{
    public class CreateMessageTemplateRequestDto
    {
        public CreateMessageTemplateRequestDto()
        {
            Buttons = new List<ButtonDto>();
        }

        public string ClientId { get; set; }
        public string SenderNameId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string LanguageCode { get; set; }

        public HeaderDto Header { get; set; }
        public BodyDto Body { get; set; }
        public FooterDto Footer { get; set; }
        public List<ButtonDto> Buttons { get; set; }
        public FlowComponent Flow { get; set; }

        public class HeaderDto
        {
            public HeaderDto()
            {
            }

            public string Format { get; set; }
            public string MediaUrl { get; set; }
            public string Text { get; set; }
            public string Example { get; set; }
        }

        public class BodyDto
        {
            public BodyDto()
            {
                Examples = new List<string>();
            }

            public string Text { get; set; }
            public List<string> Examples { get; set; }
        }

        public class FooterDto
        {
            public string Text { get; set; }
        }

        public class ButtonDto
        {
            public ButtonDto()
            {

            }

            public string Type { get; set; }
            public string Text { get; set; }
            public string PhoneNumber { get; set; }
            public string Url { get; set; }
            public string Example { get; set; }
        }

        public class FlowComponent
        {
            public string FlowId { get; set; }
            public string ButtonText { get; set; }
        }
    }
}
