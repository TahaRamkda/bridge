namespace WhatsAppBridge.Models.WhatsApp
{
    public class SendMessageTemplateModel
    {
        public string messaging_product { get; set; }
        public string recipient_type { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public Template template { get; set; }

        public class Template
        {
            public Template()
            {
                components = new List<Component>();
            }

            public string name { get; set; }
            public Language language { get; set; }
            public class Language
            {
                public string code { get; set; }
            }

            public List<Component> components { get; set; }

            public class Component
            {
                public Component()
                {
                    parameters = new List<object>();
                }

                public string type { get; set; }
                public string sub_type { get; set; }
                public int index { get; set; }

                public List<object> parameters { get; set; }
            }
        }
    }
}
