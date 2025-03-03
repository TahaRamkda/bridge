namespace WhatsAppBridge.Models.Integration.Flow
{
    public class CreateFlowRequestDto
    {
        public string ClientId { get; set; }
        public string SenderNameId { get; set; }
        public string FlowId { get; set; } //Meta flow id in case of update
        public string Name { get; set; }
        public string Category { get; set; }
        public string EndpointUrl { get; set; } 
        public string FlowJson { get; set; }
    }
}
