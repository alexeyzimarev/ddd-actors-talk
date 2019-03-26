using System;

namespace Talk.Domain
{
    public static class VehicleEvents
    {
        public class VehicleRegistered
        {
            public Guid VehicleId { get; set; }
            public string MakeModel { get; set; }
            public string Registration { get; set; }
            public string State { get; set; }
        }
    }
}