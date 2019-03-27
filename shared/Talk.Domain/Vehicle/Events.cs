namespace Talk.Domain.Vehicle
{
    public static class Events
    {
        public class VehicleRegistered
        {
            public string VehicleId { get; set; }
            public string MakeModel { get; set; }
            public string Registration { get; set; }
            public int MaxSpeed { get; set; }
            public int MaxTemperature { get; set; }
            public string State { get; set; }
        }

        public class VehicleMaxSpeedAdjusted
        {
            public string VehicleId { get; set; }
            public int MaxSpeed { get; set; }
        }

        public class VehicleMaxTemperatureAdjusted
        {
            public string VehicleId { get; set; }
            public int MaxTemperature { get; set; }
        }
    }
}