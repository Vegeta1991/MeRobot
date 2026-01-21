namespace MeRobot.Models
{
    public class RobotState
    {
        public int BatteryVoltageMv { get; set; }
        public int BatteryTemperatureC { get; set; }
        public bool BumpDetected { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }
}
