using API.Commons;
using System.Text.Json;

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
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);
                        
            if ((context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                && context.Response.ContentType != "text/html")
            {
                context.Response.ContentType = "application/json";

                responseBody.Seek(0, SeekOrigin.Begin);
                var readToEnd = await new StreamReader(responseBody).ReadToEndAsync();
                                
                var result = string.IsNullOrEmpty(readToEnd)
                    ? null
                    : JsonSerializer.Deserialize<object>(readToEnd);

                ApiResponse<object> finalResponse;
                if (result?.ToString() == "False")
                {
                    finalResponse = new ApiResponse<object>();
                }
                else
                {
                    finalResponse = new ApiResponse<object>(result);
                }
                   
                var jsonResponse = JsonSerializer.Serialize(finalResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                                
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
                await originalBodyStream.WriteAsync(bytes, 0, bytes.Length);
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
