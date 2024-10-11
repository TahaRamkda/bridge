namespace WhatsAppBridge.Settings
{
    public class WhatsAppConfigurationSetting
    {
        public const string ConfigKey = "WhatsAppConfiguration";
        public string WebhookVerificationToken { get; set; }
        public string BaseURL { get; set; }
        public string AccessToken { get; set; }
        public int TimeOutInSeconds { get; set; }
    }
}
