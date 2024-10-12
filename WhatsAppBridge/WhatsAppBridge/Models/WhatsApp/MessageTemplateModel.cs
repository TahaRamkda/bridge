namespace WhatsAppBridge.Models.WhatsApp
{
    public class MessageTemplateModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string sub_category { get; set; }
        public string language { get; set; }
        public string status { get; set; }
        public List<Component> components { get; set; }

        public class Component
        {
            public string type { get; set; }
            public string format { get; set; }
            public string text { get; set; }
            public dynamic example { get; set; }
            public dynamic buttons { get; set; }
        }
    }
}
