namespace WhatsAppBridge.Models.Integration
{
    public class SendMessageResponseDto
    {
        public int Status { get; set; }
        public string PhoneNumber { get; set; }
        public string WAId { get; set; }
        public string MessageId { get; set; }
    }
}
