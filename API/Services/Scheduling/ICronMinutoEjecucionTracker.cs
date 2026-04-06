namespace API.Services.Scheduling;

/// <summary>
/// Evita lanzar dos veces el mismo trabajo programado en el mismo minuto UTC (p. ej. si el tick se repite antes de que cambie el minuto).
/// </summary>
public interface ICronMinutoEjecucionTracker
{
    bool TryBeginEjecucionEsteMinutoUtc(int trabajoId, DateTime utcNow);
}
