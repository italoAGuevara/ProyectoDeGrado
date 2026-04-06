using API.Services.Interfaces;

namespace API.Endpoints;

public static class ReportesEndpoint
{
    public static void MapReportesEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/reportes").RequireAuthorization();

        group.MapGet("/ejecuciones", GetEjecucionesHistorial)
            .WithName("GetEjecucionesHistorial");
    }

    private static async Task<IResult> GetEjecucionesHistorial(
        IJobExecutionReportService reportes,
        int? trabajoId,
        DateTime? desdeUtc,
        DateTime? hastaUtc,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await reportes.GetHistorialAsync(trabajoId, desdeUtc, hastaUtc, page, pageSize, cancellationToken);
        return Results.Ok(result);
    }
}
