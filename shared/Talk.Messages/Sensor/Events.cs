namespace Talk.Messages.Sensor
{
    public static class Events
    {
        public class SensorInstalled
        {
            public string SensorId { get; set; }
            public string VehicleId { get; set; }

            public override string ToString()
                => $"Sensor {SensorId} installed to vehicle {VehicleId}";
        }

        public class SensorTelemetryReceived
        {
            public string SensorId { get; set; }
            public string VehicleId { get; set; }
            public int Speed { get; set; }
            public int Temperature { get; set; }

            public override string ToString()
                => $"Received telemetry from Sensor {SensorId}. Speed {Speed}, temp {Temperature}";
        }
    }
}