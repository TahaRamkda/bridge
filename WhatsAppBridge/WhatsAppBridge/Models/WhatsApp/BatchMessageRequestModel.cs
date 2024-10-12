namespace WhatsAppBridge.Models.WhatsApp
{
    public class BatchMessageRequestModel
    {
        public BatchMessageRequestModel()
        {
            batch = new List<Batch>();
        }

        public List<Batch> batch { get; set; }

        public class Batch
        {
            public string method { get; set; }
            public string relative_url { get; set; }
            public string body { get; set; }
        }
    }
}

