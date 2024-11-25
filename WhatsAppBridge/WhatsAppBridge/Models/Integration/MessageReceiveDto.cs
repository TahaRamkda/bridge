namespace WhatsAppBridge.Models.Integration
{
    public class MessageReceiveDto
    {
        public string client_Id { get; set; }
        public string wam_Id { get; set; }
        public string update_dateTime { get; set; }
        public string from { get; set; }
        public string type { get; set; }
        public PhoneNumber phone_number_Id { get; set; }
        public Context context { get; set; }
        public Button button { get; set; }
        public Text text { get; set; }
        public Image image { get; set; }
        public Document document { get; set; }
<<<<<<< HEAD
        public Location location { get; set; }
        public Sticker sticker { get; set; }
=======
        public Video video { get; set; }
        public PhoneNumber phone_number_Id { get; set; }
>>>>>>> 418c51ac2d6d1dcdacc0d0acf18b94ab9b425a26

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
    }
}
