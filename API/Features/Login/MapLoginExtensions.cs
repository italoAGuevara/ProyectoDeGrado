using Microsoft.AspNetCore.Builder;

namespace API.Features.Login
{
    public static class MapLoginExtensions
    {
        public static void MapLogin(this WebApplication app)
        {
            var loginApi = app.MapGroup("/api/login");
            loginApi.MapPost("/", async (ValiadateJwtRequest request, AppDbContext context) =>
            {
                var handler = new LoginHandler(context);
                return Results.Ok(await handler.Handle(request));
            })
            .WithName("Login");
        }
    }
}
