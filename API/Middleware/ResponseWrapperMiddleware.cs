using API.Commons;
using System.Text.Json;
using System.Text;

namespace API.Middleware
{
    public class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseWrapperMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // OpenAPI/Swagger JSON must stay unwrapped so Scalar and other tools receive a valid document.
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            if (context.Response.StatusCode == 204)
            {
                // 204 No Content no puede tener cuerpo
                context.Response.Body = originalBodyStream;
            }
            else if ((context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                && context.Response.ContentType != null && context.Response.ContentType.Contains("application/json"))
            {
                context.Response.ContentType = "application/json";

                responseBody.Seek(0, SeekOrigin.Begin);
                var readToEnd = await new StreamReader(responseBody).ReadToEndAsync();

                object? result = null;
                if (!string.IsNullOrWhiteSpace(readToEnd))
                {
                    try
                    {
                        result = JsonSerializer.Deserialize<object>(readToEnd);
                    }
                    catch (JsonException)
                    {
                        // Si no es un JSON válido, dejamos el resultado como null
                    }
                }

                var finalResponse = new ApiResponse<object>(result);
                var jsonResponse = JsonSerializer.Serialize(finalResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var bytes = Encoding.UTF8.GetBytes(jsonResponse);
                await originalBodyStream.WriteAsync(bytes);
            }
            else
            {
                // Si es un error (400, 500, etc.), dejamos que pase tal cual o lo maneje otro middleware
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
