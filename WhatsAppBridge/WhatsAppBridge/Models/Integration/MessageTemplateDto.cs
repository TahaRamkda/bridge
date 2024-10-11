namespace WhatsAppBridge.Models.Integration
{
    public partial class MessageTemplateDto
    {
        public MessageTemplateDto()
        {
            Buttons = new List<ButtonComponent>();
        }

        public string Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Category { get; set; } = String.Empty;
        public string SubCategory { get; set; } = String.Empty;
        public string Language { get; set; } = String.Empty;
        public string Status { get; set; } = String.Empty;
        public bool IsApproved { get; set; }
        public HeaderComponent Header { get; set; }
        public BodyComponent Body { get; set; }
        public FooterComponent Footer { get; set; }
        public List<ButtonComponent> Buttons { get; set; }

        public partial class HeaderComponent
        {
            public HeaderComponent()
            {
                Values = new List<string>();
            }

            public string Format { get; set; } = String.Empty;
            public string Text { get; set; } = String.Empty;
            public int TextCount { get; set; }
            public List<string> Values { get; set; }
        }

        public partial class BodyComponent
        {
            public BodyComponent()
            {
                Values = new List<string>();
            }

            public string Text { get; set; } = String.Empty;
            public int TextCount { get; set; }
            public List<string> Values { get; set; }
        }

        public partial class FooterComponent
        {
            public string Text { get; set; } = String.Empty;
        }

        public partial class ButtonComponent
        {
            public ButtonComponent()
            {
                Values = new List<string>();
            }

            public string Type { get; set; }
            public string Text { get; set; } = String.Empty;
            public string PhoneNumber { get; set; } = String.Empty;
            public int TextCount { get; set; }
            public string Url { get; set; } = String.Empty;
            public List<string> Values { get; set; }
        }
    }
}
