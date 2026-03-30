using API.Services.Interfaces;

namespace API.Endpoints;

public static class LogAccionesUsuarioEndpoint
{
    public static void MapLogAccionesUsuario(this WebApplication app)
    {
        var group = app.MapGroup("/api/log-acciones-usuario").RequireAuthorization();

        group.MapGet("/", Listar)
            .WithName("GetLogAccionesUsuario");
    }

    private static async Task<IResult> Listar(
        int? limite,
        ILogAccionesUsuarioQueryService queryService)
    {
        var list = await queryService.ListarAsync(limite);
        return Results.Ok(list);
    }
}
