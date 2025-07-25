namespace WhatsAppBridge.Models.Integration
{
    public class AnalyticsResponseDto
    {
        public string ClientId { get; set; }
        public string SenderId { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public List<string> PhoneNumber { get; set; }
        public long sent { get; set; }
        public long delivered { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
    }
}
