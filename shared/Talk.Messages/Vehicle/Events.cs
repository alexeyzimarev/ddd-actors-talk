namespace Talk.Messages.Vehicle
{
    public static class Events
    {
        public class VehicleRegistered
        {
            public string VehicleId { get; set; }
            public string CustomerId { get; set; }
            public string MakeModel { get; set; }
            public string Registration { get; set; }
            public int MaxSpeed { get; set; }
            public int MaxTemperature { get; set; }
            public string State { get; set; }

            public override string ToString()
                => $"Vehicle {Registration} with id {VehicleId} registered for customer {CustomerId}";
        }

        public class VehicleMaxSpeedAdjusted
        {
            public string VehicleId { get; set; }
            public int MaxSpeed { get; set; }

            public override string ToString()
                => $"Max speed for vehicle {VehicleId} adjusted to {MaxSpeed}";
        }

        public class VehicleMaxTemperatureAdjusted
        {
            public string VehicleId { get; set; }
            public int MaxTemperature { get; set; }

            public override string ToString()
                => $"Max temperature for vehicle {VehicleId} adjusted to {MaxTemperature}";
        }

        public class VehicleOverheated
        {
            public string VehicleId { get; set; }
            public int Temperature { get; set; }

            public override string ToString()
                => $"Vehicle {VehicleId} is overheated, temperature {Temperature}";
        }

        public class VehicleSpeeingDetected
        {
            public string VehicleId { get; set; }
            public int RecordedSpeed { get; set; }

            public override string ToString()
                => $"Vehicle {VehicleId} is speeding, speed {RecordedSpeed}";
        }
    }
}