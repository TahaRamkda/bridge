namespace WhatsAppBridge.Models.Integration
{
    public class AnalyticsRequestDto
    {
        public string ClientId { get; set; }
        public string SenderId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
