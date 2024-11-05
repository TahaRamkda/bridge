namespace WhatsAppBridge.Models.Integration
{
    public class MessageReceiveDto
    {
        public string client_Id { get; set; }
        public string wam_Id { get; set; }
        public string update_dateTime { get; set; }
        public string from { get; set; }
        public string type { get; set; }
        public Context context { get; set; }
        public Button button { get; set; }
        public Text text { get; set; }
        public PhoneNumber phone_number_Id { get; set; }

        public class PhoneNumber
        {
            public string display_phone_number { get; set; }
            public string phone_number_id { get; set; }
        }

        public class Context
        {
            public string from { get; set; }
            public string wam_Id { get; set; }
        }

        public class Button
        {
            public string payload { get; set; }
            public string text { get; set; }
        }

        public class Text
        {
            public string body { get; set; }
        }
    }
}
