using Talk.EventStore;
using static Talk.Messages.Vehicle.Events;

namespace Talk.EsBase.Commands.Modules.Vehicles
{
    public static class EventMapping
    {
        public static void Map()
        {
            TypeMapper.Map<VehicleRegistered>("VehicleRegistered");
            TypeMapper.Map<VehicleMaxSpeedAdjusted>("VehicleMaxSpeedAdjusted");
            TypeMapper.Map<VehicleMaxTemperatureAdjusted>("VehicleMaxTemperatureAdjusted");
            TypeMapper.Map<VehicleSpeeingDetected>("VehicleSpeedingDetected");
            TypeMapper.Map<VehicleOverheated>("VehicleOverheated");
        }
    }
}