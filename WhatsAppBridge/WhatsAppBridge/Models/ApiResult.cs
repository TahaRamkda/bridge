namespace WhatsAppBridge.Models
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public Object Result { get; set; }
        public string Message { get; set; } = "";
        public int StatusCode { get; set; }
    }
}
