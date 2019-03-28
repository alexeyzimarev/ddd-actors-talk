using Talk.Domain.Vehicle;
using Talk.EsBase.Server.Infrastructure.EventStore;
using Talk.Messages.Vehicle;

namespace Talk.EsBase.Server.Modules.Vehicles
{
    public static class EventMapping
    {
        public static void Map()
        {
            TypeMapper.Map<Events.VehicleRegistered>("VehicleRegistered");
            TypeMapper.Map<Events.VehicleMaxSpeedAdjusted>("VehicleMaxSpeedAdjusted");
            TypeMapper.Map<Events.VehicleMaxTemperatureAdjusted>("VehicleMaxTemperatureAdjusted");
        }
    }
}