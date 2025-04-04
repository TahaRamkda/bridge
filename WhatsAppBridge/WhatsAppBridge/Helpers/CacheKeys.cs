namespace WhatsAppBridge.Helpers
{
    public static class CacheKeys
    {
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string CLIENTS_PATTERN_KEY => "Bridge.Client.";
         
        /// <summary>
        /// {0} - ClientId
        /// </summary>
        public static readonly string CLIENTS_BY_ID_KEY = "Bridge.Client.Id-{0}";

        /// <summary>
        /// </summary>
        public static readonly string CLIENTS_DROPDOWN_KEY = "Bridge.Client.dropdown";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string SENDERS_DROPDOWN_KEY = "Bridge.Sender.dropdown-{0}";


        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public static string SENDERS_PATTERN_KEY => "Bridge.Sender.";
 
        /// <summary>
        /// {0} - ClientId 
        /// {1} - SenderId
        /// </summary>
        public static readonly string SENDERS_BY_CLIENTID_SENDERID_KEY = "Bridge.Sender.{0}-{1}";
    }
}
