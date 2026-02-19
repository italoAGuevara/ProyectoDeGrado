
namespace API.Features.Login
{
    public static class MapIsTokenValidExtensions
    {
        public static void MapValidateJwt(this WebApplication app)
        {
            var loginApi = app.MapGroup("/api/IsTokenValid");
            loginApi.MapGet("/", async (HttpRequest httpRequest, AppDbContext context) =>
            {                
                var authHeader = httpRequest.Headers["Authorization"].FirstOrDefault();
                string token = string.Empty;
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }

                var handler = new IsTokenValidHandler(context);
                return Results.Ok(await handler.Handle(token));
            })
            .WithName("IsTokenValid");
        }
    }
}
