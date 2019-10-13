using static Talk.Messages.Vehicle.Events;

namespace Talk.Domain.Vehicle
{
    public static class Vehicle
    {
        public static VehicleState.Result Register(
            string vehicleId,
            string customerId,
            string makeModel,
            string registration,
            int maxSpeed,
            int maxTemperature
        ) => new VehicleState()
            .Apply(new VehicleRegistered
            {
                VehicleId      = vehicleId,
                CustomerId     = customerId,
                MakeModel      = makeModel,
                Registration   = registration,
                MaxSpeed       = maxSpeed,
                MaxTemperature = maxTemperature,
                State          = "Just registered"
            });

        public static VehicleState.Result AdjustMaxSpeed(VehicleState state, int maxSpeed)
            => state
                .Apply(
                    new VehicleMaxSpeedAdjusted
                    {
                        VehicleId = state.Id,
                        MaxSpeed  = maxSpeed
                    });

        public static VehicleState.Result AdjustMaxTemperature(VehicleState state, int maxTemperature)
            => state
                .Apply(
                    new VehicleMaxTemperatureAdjusted
                    {
                        VehicleId      = state.Id,
                        MaxTemperature = maxTemperature
                    });

        public static VehicleState.Result ProcessTelemetry(VehicleState state, int speed, int temperature)
            => state switch
            {
                { } s when speed > s.MaxSpeed => state.Apply(
                    new VehicleSpeeingDetected
                    {
                        VehicleId     = state.Id,
                        RecordedSpeed = speed
                    }),
                { } s when temperature > s.MaxTemperature => state.Apply(
                    new VehicleOverheated
                    {
                        VehicleId   = state.Id,
                        Temperature = temperature
                    }
                ),
                _ => state.EmptyResult()
            };
    }
}