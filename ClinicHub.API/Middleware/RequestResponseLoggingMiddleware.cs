using System.Text;

namespace ClinicHub.API.Middleware
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
            // Only log for API endpoints to avoid noise
            if (!context.Request.Path.Value!.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // 1. Capture Request Details
            var requestBody = await ReadRequestBodyAsync(context.Request);
            var requestHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            // 2. Capture Response Body by swapping the stream
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            try
            {
                await _next(context);

                // 3. Capture Response Details
                var responseBody = await ReadResponseBodyAsync(context.Response);
                var responseHeaders = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

                // 4. Log everything in a structured format
                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode}\n" +
                    "--- REQUEST ---\n" +
                    "Headers: {@RequestHeaders}\n" +
                    "Body: {RequestBody}\n" +
                    "--- RESPONSE ---\n" +
                    "Headers: {@ResponseHeaders}\n" +
                    "Body: {ResponseBody}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    requestHeaders,
                    requestBody,
                    responseHeaders,
                    responseBody);

                // 5. Copy the captured response back to the original stream
                responseBodyMemoryStream.Position = 0;
                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyStream);
            }
            finally
            {
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();

            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return string.IsNullOrWhiteSpace(body) ? "[Empty]" : body;
        }

        private async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return string.IsNullOrWhiteSpace(body) ? "[Empty]" : body;
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
