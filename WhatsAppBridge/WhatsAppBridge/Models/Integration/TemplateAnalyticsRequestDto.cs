namespace WhatsAppBridge.Models.Integration
{
    public class TemplateAnalyticsRequestDto
    {
        public string ClientId { get; set; }
        public string SenderId { get; set; }
        public string TemplateId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
