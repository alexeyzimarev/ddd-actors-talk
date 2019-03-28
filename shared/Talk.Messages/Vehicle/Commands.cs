namespace Talk.Messages.Vehicle
{
    public static class Commands
    {
        public class RegisterVehicle
        {
            public string VehicleId { get; set; }
            public string CustomerId { get; set; }
            public string Registration { get; set; }
            public string MakeModel { get; set; }
            public int MaxSpeed { get; set; }
            public int MaxTemperature { get; set; }

            public override string ToString()
                => $"Register vehicle {VehicleId} for customer {CustomerId}";
        }

        public class AdjustMaxSpeed
        {
            public string VehicleId { get; set; }
            public int MaxSpeed { get; set; }

            public override string ToString()
                => $"Adjust max speed for {VehicleId} to {MaxSpeed}";
        }

        public class AdjustMaxTemperature
        {
            public string VehicleId { get; set; }
            public int MaxTemperature { get; set; }

            public override string ToString()
                => $"Adjust max temperature for {VehicleId} to {MaxTemperature}";
        }

        public class RegisterVehicleTelemetry
        {
            public string VehicleId { get; set; }
            public string SensorId { get; set; }
            public int Speed { get; set; }
            public int Temperature { get; set; }
        }
    }
}