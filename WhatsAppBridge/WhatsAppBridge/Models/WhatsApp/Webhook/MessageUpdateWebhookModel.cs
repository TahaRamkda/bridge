
namespace WhatsAppBridge.Models.WhatsApp.Webhook
{
    public class MessageUpdateWebhookModel
    {
        public MessageUpdateWebhookModel()
        {
            contacts = new List<Contacts>();
            statuses = new List<Status>();
            messages = new List<Message>();
        }

        public string messaging_product { get; set; }

        public Metadata metadata { get; set; }

        public List<Contacts> contacts { get; set; }

        public List<Status> statuses { get; set; }

        public List<Message> messages { get; set; }

        public class Metadata
        {
            public string display_phone_number { get; set; }
            public string phone_number_id { get; set; }
        }

        public class Contacts
        {
            public string wa_id { get; set; }

            public Profile profile { get; set; }

            public class Profile
            {
                public string name { get; set; }
            }
        }

        public class Status
        {
            public Status()
            {
                errors = new List<Error>();
            }

            public string id { get; set; }
            public string status { get; set; }
            public long timestamp { get; set; }
            public string recipient_id { get; set; }
            public Conversation conversation { get; set; }
            public Pricing pricing { get; set; }

            public List<Error> errors { get; set; }

            public class Conversation
            {
                public string id { get; set; }

                public string expiration_timestamp { get; set; }

                public Origin origin { get; set; }

                public class Origin
                {
                    public string type { get; set; }
                }
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
                public ErrorData error_data { get; set; }

                public class ErrorData
                {
                    public string details { get; set; }
                }
            }
        }

        public class Message
        {
            public Context context { get; set; }

            public class Context
            {
                public string from { get; set; }
                public string id { get; set; }
            }

            public string from { get; set; }
            public string id { get; set; }
            public long timestamp { get; set; }
            public string type { get; set; }
            public Text text { get; set; }
            public Button button { get; set; }
            public Image image { get; set; }
            public Video video { get; set; }
            public Audio audio { get; set; }
            public Document document { get; set; }
            public Location location { get; set; }
            public Sticker sticker { get; set; }
            public Interactive interactive { get; set; }
            public Reaction reaction { get; set; }

            public class Text
            {
                public string body { get; set; }
            }

            public class Image
            {
                public string mime_type { get; set; }
                public string sha256 { get; set; }
                public string id { get; set; }
                public string caption { get; set; }
            }

            public class Video
            {
                public string mime_type { get; set; }
                public string sha256 { get; set; }
                public string id { get; set; }
                public string caption { get; set; }
            }

            public class Audio
            {
                public string mime_type { get; set; }
                public string sha256 { get; set; }
                public string id { get; set; }
                public bool voice { get; set; }
            }

            public class Button
            {
                public string payload { get; set; }
                public string text { get; set; }
            }

            public class Document
            {
                public string filename { get; set; }
                public string mime_type { get; set; }
                public string sha256 { get; set; }
                public string id { get; set; }
                public string caption { get; set; }

            }

            public class Location
            {
                public string latitude { get; set; }
                public string longitude { get; set; }
            }

            public class Sticker
            {
                public string mime_type { get; set; }
                public string sha256 { get; set; }
                public string id { get; set; }
                public string caption { get; set; }
                public bool animated { get; set; }
            }

            public class Interactive
            {
                public string type { get; set; }
                public ButtonReply button_reply { get; set; }
                public ListReply list_reply { get; set; }
                public NFMReply nfm_reply { get; set; }

                public class ButtonReply
                {
                    public string id { get; set; }
                    public string title { get; set; }
                }

                public class ListReply
                {
                    public string id { get; set; }
                    public string title { get; set; }
                }

                public class NFMReply
                {
                    public string response_json { get; set; }
                    public string body { get; set; }
                    public string name { get; set; }
                }
            }

            public class Reaction
            {
                public string message_id { get; set; }
                public string emoji { get; set; }
            }
        }
    }
}
