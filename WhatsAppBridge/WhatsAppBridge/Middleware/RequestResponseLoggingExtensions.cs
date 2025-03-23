namespace WhatsAppBridge.Middleware
{
    public static class RequestResponseLoggingExtensions
    {
        public static void UseRequestResponseLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
