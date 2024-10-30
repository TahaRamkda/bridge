namespace WhatsAppBridge.Models.WhatsApp
{
    public class CreateMessageTemplateRequestModel
    {
        public CreateMessageTemplateRequestModel()
        {
            components = new List<dynamic>();
        }

        public string name { get; set; }
        public string language { get; set; }
        public string category { get; set; }
        public bool allow_category_change { get; set; }
        public List<dynamic> components { get; set; }
    }
}
