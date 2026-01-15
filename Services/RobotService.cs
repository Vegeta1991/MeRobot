using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeRobot.Services
{
    public class RobotService : BackgroundService
    {
        private readonly ILogger<RobotService> _logger;

        public RobotService(ILogger<RobotService> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("RobotService started");

            return Task.WhenAll(
                ControlLoop(ct),
                SensorLoop(ct)
            );
        }

        // =========================
        // CONTROL LOOP (movement)
        // =========================
        private async Task ControlLoop(CancellationToken ct)
        {
            _logger.LogInformation("ControlLoop started");

            while (!ct.IsCancellationRequested)
            {
                // TODO: movement logic later
                _logger.LogInformation("ControlLoop tick");

                await Task.Delay(20, ct); // ~50 Hz
            }

            _logger.LogInformation("ControlLoop stopped");
        }

        // =========================
        // SENSOR LOOP (telemetry)
        // =========================
        private async Task SensorLoop(CancellationToken ct)
        {
            _logger.LogInformation("SensorLoop started");

            while (!ct.IsCancellationRequested)
            {
                // TODO: read sensors later
                _logger.LogInformation("SensorLoop tick");

                await Task.Delay(100, ct); // ~10 Hz
            }

            _logger.LogInformation("SensorLoop stopped");
        }
    }

}
