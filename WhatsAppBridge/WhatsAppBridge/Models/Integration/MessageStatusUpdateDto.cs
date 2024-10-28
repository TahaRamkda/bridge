namespace WhatsAppBridge.Models.Integration
{
    public class MessageStatusUpdateDto
    {
        public string client_Id { get; set; }
        public string wam_Id { get; set; }
        public string status { get; set; }
        public string update_DateTime { get; set; }
        public string recipient_Id { get; set; }
        public Conversation conversation { get; set; }
        public Pricing pricing { get; set; }
        public Error error { get; set; }

        public class Conversation
        {
            public string id { get; set; }
            public string origin_type { get; set; }
        }

        public class Pricing
        {
            public bool billable { get; set; }
            public string pricing_model { get; set; }
            public string category { get; set; }
        }

        public class Error
        {
            public string code { get; set; }
            public string title { get; set; }
            public string message { get; set; }
            public string error_Details { get; set; }
        }
    }
}

