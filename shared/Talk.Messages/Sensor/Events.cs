namespace Talk.Messages.Sensor
{
    public static class Events
    {
        public class SensorInstalled
        {
            public string SensorId { get; set; }
            public string VehicleId { get; set; }
        }

        public class TelemetryReceived
        {
            public string SensorId { get; set; }
            public int Speed { get; set; }
            public int Temperature { get; set; }
        }
    }
}