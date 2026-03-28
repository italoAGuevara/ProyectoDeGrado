using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HostedService
{
    public class Robot : BackgroundService
    {
        private readonly ILogger<Robot> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(10);

        public Robot(ILogger<Robot> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Robot starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Robot executing task in background: {time}", DateTimeOffset.Now);

                    // here starts main logic of the robot
                    await Process();
                }
                catch (TaskCanceledException)
                {
                    // Log as information instead of error when the task is canceled
                    _logger.LogInformation("Robot task was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Robot error executing task in background.");
                }

                try
                {
                    await Task.Delay(_intervalo, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Handle cancellation during delay
                    _logger.LogInformation("Robot delay was canceled.");
                }
            }
        }

        private Task Process()
        {
            
            return Task.CompletedTask;
        }
    }
}
