using System;
using Talk.EventSourcing;
using static Talk.Domain.Vehicle.Events;

namespace Talk.Domain.Vehicle
{
    public class VehicleState : AggregateState<VehicleState>
    {
        string State { get; set; }
        int MaxSpeed { get; set; }
        int MaxTemperature { get; set; }

        public override VehicleState When(VehicleState state, object @event)
            => @event switch
                {
                    VehicleRegistered e =>
                        With(state, x =>
                        {
                            x.Id = e.VehicleId;
                            x.State = "Just registered";
                            x.MaxSpeed = e.MaxSpeed;
                            x.MaxTemperature = e.MaxTemperature;
                        }),
                    VehicleMaxSpeedAdjusted e =>
                        With(state, x => x.MaxSpeed = e.MaxSpeed),
                    VehicleMaxTemperatureAdjusted e =>
                        With(state, x => x.MaxTemperature = e.MaxTemperature),
                    _ => state
                };

        protected override bool EnsureValidState(VehicleState newState)
            => newState switch
                {
                    VehicleState v when String.IsNullOrWhiteSpace(v.Id) => false,
                    _ => true
                };
    }
}