namespace WhatsAppBridge.Models.WhatsApp
{
    public class SendInteractiveMessageModel
    {
        public string messaging_product { get; set; }
        public string recipient_type { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public dynamic interactive { get; set; }
    }
}
