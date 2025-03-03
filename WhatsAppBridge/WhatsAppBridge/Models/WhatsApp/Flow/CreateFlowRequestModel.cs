namespace WhatsAppBridge.Models.WhatsApp.Flow
{
    public class CreateFlowRequestModel
    {
        public CreateFlowRequestModel()
        {
            categories = new List<string>();
        }

        public string name { get; set; }
        public List<string> categories { get; set; }
        public string endpoint_uri { get; set; }
        public string flow_json { get; set; }
    }
}
