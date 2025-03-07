using WhatsAppBridge.Models.Integration.Flow;

namespace WhatsAppBridge.Models.Integration
{
    public class MessageReceiveDto
    {
        public MessageReceiveDto()
        {
            
        }

        public string client_Id { get; set; }
        public string wam_Id { get; set; }
        public string update_dateTime { get; set; }
        public string from { get; set; }
        public string type { get; set; }
        public PhoneNumber phone_number_Id { get; set; }
        public Contact contact { get; set; }
        public Context context { get; set; }
        public Button button { get; set; }
        public Text text { get; set; }
        public Image image { get; set; }
        public Document document { get; set; }

        public Location location { get; set; }
        public Sticker sticker { get; set; }
        public Video video { get; set; }
        public Audio audio { get; set; }
        public ButtonReply buttonReply { get; set; }
        public ListReply listReply { get; set; }
        public Reaction reaction { get; set; }
        public FlowResponse flowResponse{ get; set; }

        public class PhoneNumber
        {
            public string display_phone_number { get; set; }
            public string phone_number_id { get; set; }
        }

        public class Contact
        {
            public string wa_id { get; set; }
            public string name { get; set; }
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

        public class Image
        {
            public string id { get; set; }
            public string caption { get; set; }
            public string mime_type { get; set; }
            public string sha256 { get; set; }
        }

        public class Document
        {
            public string id { get; set; }
            public string caption { get; set; }
            public string mime_type { get; set; }
            public string sha256 { get; set; }
            public string filename { get; set; }
        }

        public class Video
        {
            public string id { get; set; }
            public string caption { get; set; }
            public string mime_type { get; set; }
            public string sha256 { get; set; }
        }

        public class Audio
        {
            public string mime_type { get; set; }
            public string sha256 { get; set; }
            public string id { get; set; }
            public bool voice { get; set; }
        }

        public class Text
        {
            public string body { get; set; }
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

        public class Reaction
        {
            public string message_id { get; set; }
            public string emoji { get; set; } 
        }
    }
}
