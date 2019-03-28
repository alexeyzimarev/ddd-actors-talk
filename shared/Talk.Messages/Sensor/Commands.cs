namespace Talk.Messages.Sensor
{
    public static class Commands
    {
        public class SensorInstallation
        {
            public string SensorId { get; set; }
            public string VehicleId { get; set; }

            public override string ToString()
                => $"Install sensor {SensorId} to vehicle {VehicleId}";
        }

        public class SensorTelemetry
        {
            public string SensorId { get; set; }
            public int Speed { get; set; }
            public int Temperature { get; set; }

            public override string ToString()
                => $"Process telemetry from {SensorId}: speed {Speed}, temp {Temperature}";
        }
    }
}