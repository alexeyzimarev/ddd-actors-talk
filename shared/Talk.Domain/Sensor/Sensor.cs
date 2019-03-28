using Talk.Messages.Sensor;

namespace Talk.Domain.Sensor
{
    public static class Sensor
    {
        public static SensorState.Result Install(
            string sensorId,
            string vehicleId
        ) =>
            new SensorState()
                .Apply(new Events.SensorInstalled
                {
                    SensorId = sensorId,
                    VehicleId = vehicleId
                });

        public static SensorState.Result ReceiveTelemetry(
            SensorState state,
            int speed,
            int temperature
        ) =>
            state.Apply(new Events.SensorTelemetryReceived
            {
                SensorId = state.Id,
                VehicleId = state.VehicleId,
                Speed = speed,
                Temperature = temperature
            });
    }
}