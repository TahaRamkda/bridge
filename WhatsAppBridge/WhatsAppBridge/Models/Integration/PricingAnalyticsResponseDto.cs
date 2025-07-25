namespace WhatsAppBridge.Models.Integration
{
    public class PricingAnalyticsResponseDto
    {
        public string ClientId { get; set; }
        public string SenderId { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public int Conversation { get; set; }
        public string PhoneNumber { get; set; }
        public string PricingType { get; set; }
        public string PricingCategory { get; set; }
        public decimal Cost { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
    }
}
