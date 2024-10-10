using System.Net;
using System.Text;
using System.Text.Json;
using WhatsAppBridge.Models;

namespace WhatsAppBridge.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string requestBody = String.Empty;

            try
            {
                var path = context?.Request?.Path.Value;

                if (!String.IsNullOrWhiteSpace(path) && path.IndexOf("swagger") == -1)
                {
                    requestBody = await FormatRequest(context.Request);
                }
                 
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                string methodType = context?.Request?.Method;
                if (methodType == "GET")
                {
                    requestBody = context?.Request?.QueryString.ToUriComponent();
                    _logger.LogError("Method: {method} | Request: {request} | Exception: {exception}", methodType, requestBody, ex.ToString());
                }
                else
                {
                    _logger.LogError("Method: {method} | Request: {request} | Exception: {exception}", methodType, requestBody, ex.ToString());
                }

                await HandleExceptionMessageAsync(context, ex).ConfigureAwait(false);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;  //rewinding the stream to 0

            return bodyAsText;
        }

        private static Task HandleExceptionMessageAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            var result = JsonSerializer.Serialize(new ApiResult
            {
                StatusCode = statusCode,
                Success = false,
                Message = "Internal Error Occurred"
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(result);
        }
    }
}
