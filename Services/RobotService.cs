using MeRobot.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeRobot.Services
{
    public class RobotService : BackgroundService
    {
        private readonly ILogger<RobotService> _logger;

        private readonly RobotState _state = new();
        private readonly object _stateLock = new();

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

            var random = new Random();

            while (!ct.IsCancellationRequested)
            {
                lock (_stateLock)
                {
                    _state.BatteryVoltageMv = random.Next(13500, 16000);
                    _state.BatteryTemperatureC = random.Next(25, 45);
                    _state.BumpDetected = random.NextDouble() > 0.9;
                    _state.LastUpdatedUtc = DateTime.UtcNow;
                }

                _logger.LogDebug(
                    "Sensors updated | Voltage={voltage}mV Temp={temp}°C Bump={bump}",
                    _state.BatteryVoltageMv,
                    _state.BatteryTemperatureC,
                    _state.BumpDetected
                );

                await Task.Delay(200, ct); // 5 Hz sensor refresh
            }

            _logger.LogInformation("SensorLoop stopped");
        }

        public RobotState GetState()
        {
            lock (_stateLock)
            {
                return new RobotState
                {
                    BatteryVoltageMv = _state.BatteryVoltageMv,
                    BatteryTemperatureC = _state.BatteryTemperatureC,
                    BumpDetected = _state.BumpDetected,
                    LastUpdatedUtc = _state.LastUpdatedUtc
                };
            }
        }
    }

}
