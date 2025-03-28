namespace WhatsAppBridge.Settings
{
    public class IntegrationConfigurationSettings
    {
        public const string ConfigKey = "IntegrationConfiguration";
        public string BaseURL { get; set; }
        public int TimeOutInSeconds { get; set; }
        public string ApiKey { get; set; }
        public bool DelaySendingStatusUpdate { get; set; }
    }
}
