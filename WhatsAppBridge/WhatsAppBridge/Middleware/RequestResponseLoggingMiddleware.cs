using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WhatsAppBridge.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log request details (method, path, headers, query)
            var request = context.Request;

            var path = request.Path.Value;

            if (!String.IsNullOrWhiteSpace(path) && (path == "/" || path.Contains("swagger")))
            {
                // Continue processing the request
                await _next(context);
                return;
            }

            // Log query string and headers
            var queryString = request.QueryString.HasValue ? request.QueryString.Value : "";
            var headers = JsonConvert.SerializeObject(request.Headers);

            string requestBody = String.Empty;
            string responseBody = String.Empty;

            // Capture the original body stream to log the request body
            var originalBodyStream = context.Response.Body;
            using (var memoryStream = new MemoryStream())
            {
                // Replace the response body with a memory stream
                context.Response.Body = memoryStream;

                // If it's a POST or PUT request, we can capture the body content
                if (request.ContentLength > 0 && (request.Method == "POST" || request.Method == "PUT"))
                    requestBody = await ReadRequestBodyAsync(request);

                // Continue processing the request
                await _next(context);

                // Log response body after processing
                memoryStream.Seek(0, SeekOrigin.Begin); // Go back to the beginning of the memory stream

                // Read the response body (this is the captured content)
                responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                // Log the response time and status code
                stopwatch.Stop();

                // Write the captured response body back to the original response stream
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }

            _logger.LogInformation("Request {Method} {Path} with queryString {queryString} and requestBody {requestBody} completed in {ElapsedMilliseconds} ms with response {response}",
                    request.Method,
                    request.Path,
                    queryString,
                    requestBody,
                    stopwatch.ElapsedMilliseconds,
                    responseBody);

        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering(); // Allows reading the request body multiple times
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset stream position for further processing
                return body;
            }
        }
    }
}
