namespace Talk.EsBase.Server.Modules.Projections
{
    public static class ReadModels
    {
        public class VehicleItem
        {
            public string VehicleId { get; set; }
            public string MakeModel { get; set; }
            public string Registration { get; set; }
            public string State { get; set; }
            public int MaxSpeed { get; set; }
            public int MaxTemperature { get; set; }

            public static string GetDatabaseId(string id)
                => $"VehicleItem/{id}";
        }

        public class CustomerVehicles
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }

            public class Vehicle
            {
                public string VehicleId { get; set; }
                public string Registration { get; set; }
                public string State { get; set; }
                public int MaxSpeed { get; set; }
                public int MaxTemp { get; set; }
                public int CurrentSpeed { get; set; }
                public int CurrentTemp { get; set; }
            }

            public static string GetDatabaseId(string id)
                => $"CustomerVehicles/{id}";
        }
    }
}