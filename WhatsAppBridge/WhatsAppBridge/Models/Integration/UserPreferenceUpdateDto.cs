namespace WhatsAppBridge.Models.Integration
{
    public class UserPreferenceUpdateDto
    {
        public string client_Id { get; set; }
        public string category { get; set; }
        public bool value { get; set; } 
        public string comments { get; set; } 
        public string update_dateTime { get; set; }
        public string phone_number { get; set; }
        public PhoneNumber phone_number_Id { get; set; }

        public class PhoneNumber
        {
            public string display_phone_number { get; set; }
            public string phone_number_id { get; set; }
        } 
    }
}
