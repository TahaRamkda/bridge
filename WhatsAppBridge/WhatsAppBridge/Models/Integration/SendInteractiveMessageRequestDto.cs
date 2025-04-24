namespace WhatsAppBridge.Models.Integration
{
    public class SendInteractiveMessageRequestDto
    {
        public SendInteractiveMessageRequestDto()
        {
            PhoneNumbers = new List<string>();
            Buttons = new List<ButtonDto>();
        }

        public string ClientId { get; set; }
        public string SenderNameId { get; set; }
        public List<string> PhoneNumbers { get; set; }
        public HeaderDto Header { get; set; }
        public BodyDto Body { get; set; }
        public FooterDto Footer { get; set; }
        public bool AskForLocation { get; set; }
        public List<ButtonDto> Buttons { get; set; }
        public FlowActionDto FlowAction { get; set; }

        public class HeaderDto
        {
            public HeaderDto() { }

            public string Format { get; set; }
            public string Value { get; set; }
        }

        public class BodyDto
        {
            public BodyDto() { }

            public string Text { get; set; }
        }


        public class FooterDto
        {
            public string Text { get; set; }
        }

        public class ButtonDto
        {
            public ButtonDto() { }

            public string Id { get; set; }
            public string Type { get; set; }
            public string Text { get; set; }
            public string Url { get; set; }
        }

        public class FlowActionDto
        {
            public FlowActionDto() { }

            public string FlowId { get; set; }
            public string Version { get; set; }
            public string ButtonText { get; set; }
            public string Token { get; set; }

        }
    }
}
