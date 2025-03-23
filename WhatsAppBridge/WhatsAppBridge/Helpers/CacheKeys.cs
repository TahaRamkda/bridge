namespace WhatsAppBridge.Helpers
{
    public static class CacheKeys
    { 
        /// <summary>
        /// {0} - ClientId
        /// </summary>
        public static readonly string ClientKey = "Bridge.Client_{0}";

        /// <summary>
        /// {0} - ClientId 
        /// {1} - SenderId
        /// </summary>
        public static readonly string ClientSenderKey = "Bridge.Client_{0}_Sender_{1}";
    }
}
