using System;
using Talk.EventSourcing;

namespace Talk.Domain
{
    public class Vehicle : AggregateState<Vehicle>
    {
        string State { get; set; }

        public override Vehicle When(Vehicle state, object @event)
            => @event switch {
                VehicleEvents.VehicleRegistered e =>
                    With(state, x =>
                    {
                        x.Id = e.VehicleId;
                        x.State = "Just registered";
                    })
                };

        protected override bool EnsureValidState(Vehicle newState)
            => newState switch
                {
                    Vehicle v when v.Id == Guid.Empty => false,
                    _ => true
                };
    }
}