using Microsoft.AspNetCore.Builder;

namespace API.Features.Scripts
{
    public static class MapScriptsExtensions
    {
        public static void MapScripts(this WebApplication app)
        {
            var group = app.MapGroup("/api/scripts").RequireAuthorization();

            group.MapGet("/", async (AppDbContext context) =>
            {
                var handler = new ScriptsHandler(context);
                var list = await handler.GetAll();
                return Results.Ok(list);
            })
            .WithName("GetScripts");

            group.MapGet("/{id:int}", async (int id, AppDbContext context) =>
            {
                var handler = new ScriptsHandler(context);
                var script = await handler.GetById(id);
                return script is null ? Results.NotFound() : Results.Ok(script);
            })
            .WithName("GetScriptById");

            group.MapPost("/", async (CreateScriptRequest request, AppDbContext context) =>
            {
                var handler = new ScriptsHandler(context);
                var script = await handler.Create(request);
                return Results.Created($"/api/scripts/{script.Id}", script);
            })
            .WithName("CreateScript");

            group.MapPut("/{id:int}", async (int id, UpdateScriptRequest request, AppDbContext context) =>
            {
                var handler = new ScriptsHandler(context);
                var script = await handler.Update(id, request);
                return script is null ? Results.NotFound() : Results.Ok(script);
            })
            .WithName("UpdateScript");

            group.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
            {
                var handler = new ScriptsHandler(context);
                var deleted = await handler.Delete(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteScript");
        }
    }
}
