using API;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Settings
{
    public static class MapSettingsExtensions
    {
        /// <summary>GET /api/settings/require-auth (sin auth) — valor actual. PUT (con auth) — actualizar.</summary>
        public static void MapSettings(this WebApplication app)
        {
            app.MapGet("/api/settings/require-auth", async (AppDbContext db) =>
            {
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync();
                return Results.Ok(new RequireAuthResponse(user?.RequirePassword ?? true));
            })
            .WithName("GetRequireAuth")
            .AllowAnonymous();

            app.MapPut("/api/settings/require-auth", async (SetRequireAuthRequest request, AppDbContext db) =>
            {
                var user = await db.Users.FirstOrDefaultAsync();
                if (user is null)
                    return Results.NotFound();
                user.RequirePassword = request.RequireAuth;
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("SetRequireAuth")
            .RequireAuthorization();
        }
    }
}
