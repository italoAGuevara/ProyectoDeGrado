using HostedService.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HostedService;

public class Robot : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Robot> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);

    public Robot(IServiceScopeFactory scopeFactory, ILogger<Robot> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Robot iniciado: revisión de trabajos por cron cada {Minutos} minuto(s).", Intervalo.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ITrabajoCronTickHandler>();
                await handler.ProcessDueJobsAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Robot: tarea cancelada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Robot: error al procesar trabajos programados.");
            }

            try
            {
                await Task.Delay(Intervalo, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Robot: espera cancelada.");
            }
        }
    }
}
