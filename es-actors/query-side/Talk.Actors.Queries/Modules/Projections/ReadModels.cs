using System;
using System.Collections.Generic;

namespace Talk.Actors.Queries.Modules.Projections
{
    public static class ReadModels
    {
        public class VehicleItem
        {
            public string Id { get; set; }
            public string MakeModel { get; set; }
            public string Registration { get; set; }
            public string State { get; set; }
            public int MaxSpeed { get; set; }
            public int MaxTemperature { get; set; }
            public List<VehicleSensor> Sensors { get; set; }

            public class VehicleSensor
            {
                public string SensorId { get; set; }
                public int Speed { get; set; }
                public int Temperature { get; set; }
                public DateTimeOffset LastUpdated { get; set; }
            }

            public static string GetDatabaseId(string id)
                => $"VehicleItem/{id}";
        }

        public class CustomerVehicles
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }

            public List<Vehicle> Vehicles { get; set; }

            public class Vehicle
            {
                public string VehicleId { get; set; }
                public string Registration { get; set; }
                public string State { get; set; }
                public int MaxSpeed { get; set; }
                public int MaxTemp { get; set; }
                public int CurrentSpeed { get; set; }
                public int CurrentTemp { get; set; }
                public DateTimeOffset LastUpdated { get; set; }
            }

            public static string GetDatabaseId(string id)
                => $"CustomerVehicles/{id}";
        }
    }
}