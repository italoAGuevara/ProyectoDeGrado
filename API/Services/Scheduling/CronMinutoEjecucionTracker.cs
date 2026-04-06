namespace API.Services.Scheduling;

public sealed class CronMinutoEjecucionTracker : ICronMinutoEjecucionTracker
{
    private readonly object _lock = new();
    private readonly Dictionary<int, long> _ultimoMinutoTicksPorTrabajo = new();

    public bool TryBeginEjecucionEsteMinutoUtc(int trabajoId, DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            utcNow = utcNow.ToUniversalTime();

        var minuteTicks = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0, DateTimeKind.Utc).Ticks;

        lock (_lock)
        {
            if (_ultimoMinutoTicksPorTrabajo.TryGetValue(trabajoId, out var prev) && prev == minuteTicks)
                return false;

            _ultimoMinutoTicksPorTrabajo[trabajoId] = minuteTicks;
            return true;
        }
    }
}
