using System;
using Talk.EventSourcing;
using static Talk.Domain.Sensor.Events;

namespace Talk.Domain.Sensor
{
    public class SensorState : AggregateState<SensorState>
    {
        string VehicleId { get; set; }
        int Speed { get; set; }
        int Temperature { get; set; }

        public override SensorState When(SensorState state, object @event)
            => @event switch
                {
                    SensorInstalled e =>
                        With(state, x =>
                        {
                            x.Id = e.SensorId;
                            x.VehicleId = e.VehicleId;
                        }),
                    TelemetryReceived e =>
                        With(state, x =>
                        {
                            x.Speed = e.Speed;
                            x.Temperature = e.Temperature;
                        }),
                    _ => state
                };

        protected override bool EnsureValidState(SensorState newState)
            => !String.IsNullOrWhiteSpace(newState.Id)
               && !String.IsNullOrWhiteSpace(newState.VehicleId);
    }
}