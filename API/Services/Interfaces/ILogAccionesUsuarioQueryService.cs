using API.DTOs;

namespace API.Services.Interfaces;

public interface ILogAccionesUsuarioQueryService
{
    /// <summary>Registros más recientes primero. Límite por defecto 500, máximo 2000.</summary>
    Task<IReadOnlyList<LogAccionesUsuarioResponse>> ListarAsync(int? limite = null, CancellationToken cancellationToken = default);
}
