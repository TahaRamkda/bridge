namespace WhatsAppBridge.Models.WhatsApp.Webhook
{
    public class UserPreferenceUpdateWebhookModel
    {
        public UserPreferenceUpdateWebhookModel()
        {
            contacts = new List<Contacts>();
            user_preferences = new List<UserPreference>();
        }

        public string messaging_product { get; set; }

        public Metadata metadata { get; set; }

        public List<Contacts> contacts { get; set; }

        public List<UserPreference> user_preferences { get; set; }

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

        public class UserPreference
        {
            public string wa_id { get; set; }
            public string detail { get; set; }
            public string category { get; set; }
            public string value { get; set; }
            public long timestamp { get; set; }
        }


    }
}
