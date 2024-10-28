namespace WhatsAppBridge.Models.Integration
{
    public class SendMessageRequestDto
    {
        public SendMessageRequestDto()
        {
            PhoneNumbers = new List<string>();
        }

        public string ClientId { get; set; }
        public string PhoneId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string MediaId { get; set; }
        public string FileName { get; set; }
        public List<string> PhoneNumbers { get; set; }
    }
}
