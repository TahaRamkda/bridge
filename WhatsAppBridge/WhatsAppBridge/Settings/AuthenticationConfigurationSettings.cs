namespace WhatsAppBridge.Settings
{
    public class AuthenticationConfigurationSettings
    {
        public const string ConfigKey = "AuthenticationConfiguration";
        public bool Authenticate { get; set; }
        public string ApiKey { get; set; }
    }
}
