namespace WhatsAppBridge.Settings
{
    public class CacheSettings
    {
        public const string ConfigKey = "CacheSettings";
        public bool CachingEnabled { get; set; }
        public int DefaultExpirationInMinutes { get; set; }
    }
}
