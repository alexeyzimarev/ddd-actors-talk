using Talk.EventSourcing;
using static System.String;
using static Talk.Messages.Sensor.Events;

namespace Talk.Domain.Sensor
{
    public class SensorState : AggregateState<SensorState>
    {
        internal string VehicleId { get; private set; }
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
                    SensorTelemetryReceived e =>
                        With(state, x =>
                        {
                            x.Speed = e.Speed;
                            x.Temperature = e.Temperature;
                        }),
                    _ => state
                };

        public override string GetStreamName(string id)
            => $"Sensor-{id}";

        protected override bool EnsureValidState(SensorState newState)
            => !IsNullOrWhiteSpace(newState.Id)
               && !IsNullOrWhiteSpace(newState.VehicleId);
    }
}