using Talk.Messages.Vehicle;

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
            .Apply(new Events.VehicleRegistered
            {
                VehicleId = vehicleId,
                CustomerId = customerId,
                MakeModel = makeModel,
                Registration = registration,
                MaxSpeed = maxSpeed,
                MaxTemperature = maxTemperature
            });

        public static VehicleState.Result AdjustMaxSpeed(
            VehicleState state,
            int maxSpeed
        ) => state
            .Apply(
                new Events.VehicleMaxSpeedAdjusted
                {
                    VehicleId = state.Id,
                    MaxSpeed = maxSpeed
                });

        public static VehicleState.Result AdjustMaxTemperature(
            VehicleState state,
            int maxTemperature
        ) => state
            .Apply(
                new Events.VehicleMaxTemperatureAdjusted
                {
                    VehicleId = state.Id,
                    MaxTemperature = maxTemperature
                });
    }
}