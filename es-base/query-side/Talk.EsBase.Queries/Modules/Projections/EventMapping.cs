using Talk.EsBase.Queries.Infrastructure.EventStore;

namespace Talk.EsBase.Queries.Modules.Projections
{
    public static class EventMapping
    {
        public static void Map()
        {
            // Customers
            TypeMapper.Map<Messages.Customer.Events.CustomerRegistered>("CustomerRegistered");

            // Vehicles
            TypeMapper.Map<Messages.Vehicle.Events.VehicleRegistered>("VehicleRegistered");
            TypeMapper.Map<Messages.Vehicle.Events.VehicleMaxSpeedAdjusted>("VehicleMaxSpeedAdjusted");
            TypeMapper.Map<Messages.Vehicle.Events.VehicleMaxTemperatureAdjusted>("VehicleMaxTemperatureAdjusted");
            TypeMapper.Map<Messages.Vehicle.Events.VehicleSpeeingDetected>("VehicleSpeedingDetected");
            TypeMapper.Map<Messages.Vehicle.Events.VehicleOverheated>("VehicleOverheated");

            // Sensors
            TypeMapper.Map<Messages.Sensor.Events.SensorInstalled>("SensorInstalled");
            TypeMapper.Map<Messages.Sensor.Events.SensorTelemetryReceived>("SensorTelemetryReceived");
        }
    }
}