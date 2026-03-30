using API.DTOs;
using API.Services.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Services;

public class LogAccionesUsuarioQueryService : ILogAccionesUsuarioQueryService
{
    private const int LimitePredeterminado = 500;
    private const int LimiteMaximo = 2000;

    private readonly AppDbContext _context;

    public LogAccionesUsuarioQueryService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<LogAccionesUsuarioResponse>> ListarAsync(
        int? limite = null,
        CancellationToken cancellationToken = default)
    {
        var n = limite is null or <= 0 ? LimitePredeterminado : Math.Min(limite.Value, LimiteMaximo);

        var rows = await _context.LogAccionesUsuario
            .AsNoTracking()
            .OrderByDescending(x => x.FechaAccion)
            .Take(n)
            .Select(x => new LogAccionesUsuarioResponse(
                x.Id.ToString(),
                x.FechaAccion,
                x.ValorAnterior,
                x.ValorNuevo,
                x.Accion,
                x.TablaAfectada))
            .ToListAsync(cancellationToken);

        return rows;
    }
}
