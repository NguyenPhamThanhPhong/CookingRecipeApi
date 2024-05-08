using System.Text;

namespace CookingRecipeApi.Helper
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Capture request information
            var request = context.Request;
            var requestBody = await ReadRequestBody(request);
            var requestHeaders = request.Headers;

            // Log request details
            _logger.LogInformation($"Request: {request.Method} {request.Path} {request.QueryString}");
            _logger.LogInformation("Headers:");
            foreach (var header in requestHeaders)
            {
                _logger.LogInformation($"\t{header.Key}: {header.Value}");
            }
            _logger.LogInformation($"Request Body: {requestBody}");

            await _next(context);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            // Ensure the request body can be read multiple times
            request.EnableBuffering();

            using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                // Reset the request body position to allow further reading in the pipeline
                request.Body.Position = 0;
                return body;
            }
        }
    }
}
