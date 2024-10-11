namespace WhatsAppBridge.Helpers
{
    public static class CommonFunction
    {
        public static string GetFullUrl(string baseUrl, string requestUri)
        {
            return String.Concat(baseUrl, requestUri);
        }
    }
}
