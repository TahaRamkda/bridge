namespace WhatsAppBridge.Models.Integration
{
    public class SendMessageResponseDto
    {
        public SendMessageResponseDto()
        {
            Errors = new List<string>();
        }

        public bool Success { get; set; }
        public int Status { get; set; }
        public string PhoneNumber { get; set; }
        public string WAId { get; set; }
        public string MessageId { get; set; }
        public List<string> Errors { get; set; }
    }
}
