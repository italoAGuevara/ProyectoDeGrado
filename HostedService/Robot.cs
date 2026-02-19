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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Robot error executing task in background.");
                }

                
                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private Task Process()
        {
            
            return Task.CompletedTask;
        }
    }
}
