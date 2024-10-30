namespace WhatsAppBridge.Models.WhatsApp
{
    public class CreateMessageTemplateResponseModel
    {
        public string id { get; set; }
        public string status { get; set; }
        public string category { get; set; }
        public Error error { get; set; }
        public class Error
        {
            public string message { get; set; }
            public string type { get; set; }
            public string code { get; set; }
            public string error_subcode { get; set; }
            public string error_user_title { get; set; }
            public string error_user_msg { get; set; }
            public string fbtrace_id { get; set; }
        }
    }

    public class UpdateMessageTemplateResponseModel
    {
        public bool success { get; set; }
        public Error error { get; set; }
        public class Error
        {
            public string message { get; set; }
            public string type { get; set; }
            public string code { get; set; }
            public string error_subcode { get; set; }
            public string error_user_title { get; set; }
            public string error_user_msg { get; set; }
            public string fbtrace_id { get; set; }
        }
    }
}
