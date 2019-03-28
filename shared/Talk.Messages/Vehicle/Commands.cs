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
        }

        public class AdjustMaxSpeed
        {
            public string VehicleId { get; set; }
            public int MaxSpeed { get; set; }
        }

        public class AdjustMaxTemperature
        {
            public string VehicleId { get; set; }
            public int MaxTemperature { get; set; }
        }
    }
}