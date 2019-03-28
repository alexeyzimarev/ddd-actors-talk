using Talk.EsBase.Commands.Infrastructure.EventStore;
using Talk.Messages.Vehicle;

namespace Talk.EsBase.Commands.Modules.Vehicles
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