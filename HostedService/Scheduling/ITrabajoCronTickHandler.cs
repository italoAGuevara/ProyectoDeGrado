namespace HostedService.Scheduling;

/// <summary>
/// Procesa trabajos activos cuya expresión cron corresponde al instante actual (UTC).
/// </summary>
public interface ITrabajoCronTickHandler
{
    Task ProcessDueJobsAsync(CancellationToken cancellationToken = default);
}
