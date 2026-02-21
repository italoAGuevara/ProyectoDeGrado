using Microsoft.AspNetCore.Builder;

namespace API.Features.Login
{
    public static class MapChangePasswordExtensions
    {
        public static void MapChangePassword(this WebApplication app)
        {
            var group = app.MapGroup("/api/user").RequireAuthorization();
            group.MapPut("/change-password", async (ChangePasswordRequest request, AppDbContext context) =>
            {
                var handler = new ChangePasswordHandler(context);
                await handler.Handle(request);
                return Results.NoContent();
            })
            .WithName("ChangePassword");
        }
    }
}
