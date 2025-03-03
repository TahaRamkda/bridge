namespace WhatsAppBridge.Models.Integration
{
    public class ClientInformationDto
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string AppId { get; set; }
        public string BusinessId { get; set; }
        public string AccessToken { get; set; }
    }

    public class SenderNameInformationDto
    {
        public string ClientId { get; set; }
        public string AppId { get; set; }
        public string BusinessId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string PhoneNumberId { get; set; }
        public string PhoneNumber { get; set; }
        public string BusinessAccountId { get; set; }
        public string AccessToken { get; set; }
        public string PublicCertificate { get; set; }
        public string PrivateCertificate { get; set; }
    }
}
