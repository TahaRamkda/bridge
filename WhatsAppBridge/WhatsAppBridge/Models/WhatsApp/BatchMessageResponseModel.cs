namespace WhatsAppBridge.Models.WhatsApp
{
    public class BatchMessageResponseModel
    {
        public BatchMessageResponseModel()
        {
        }

        public int code { get; set; }
        public string body { get; set; }
        public  BodyResponse bodyResponse { get; set; }
        public class BodyResponse
        {
            public BodyResponse()
            {
                contacts = new List<Contact>();
                messages = new List<Message>();
            }

            public string messaging_product { get; set; }
            public List<Contact> contacts { get; set; }
            public List<Message> messages { get; set; }

            public class Contact
            {
                public string input { get; set; }
                public string wa_id { get; set; }
            }

            public class Message
            {
                public string id { get; set; }
            }
        }
    }
}
