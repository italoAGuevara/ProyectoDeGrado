using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;

namespace API.Endpoints
{
    public static class AuthEndpoint
    {
        public static void MapAuthEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth").RequireAuthorization();

            group.MapPost("/login", Login)
                .WithName("Login")
                .AllowAnonymous();

            group.MapPut("/change-password", ChangePassword)
            .WithName("ChangePassword");

            group.MapGet("/is-valid-token", isTokenValid)
                .WithName("IsTokenValid");
        }

        private static async Task<IResult> Login(DTOs.LoginRequest request, ILogin loginService)
        {
            var token = await loginService.LoginUser(request.password);
            return Results.Ok(token);
        }


        private static async Task<IResult> ChangePassword(ChangePasswordRequest request, ILogin loginService)
        {
            await loginService.ChangePassword(request.CurrentPassword, request.NewPassword);
            return Results.Ok(string.Empty);
        }


        private static async Task<IResult> isTokenValid(HttpRequest httpRequest, ILogin loginService)
        {
            var authHeader = httpRequest.Headers["Authorization"].FirstOrDefault();
            string token = string.Empty;
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }

            return Results.Ok(await loginService.isTokenValid(token));
        }
    }
}
