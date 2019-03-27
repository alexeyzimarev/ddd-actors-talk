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

            public static string GetDatabaseId(string id)
                => $"VehicleItem/{id}";
        }
    }
}