using Talk.Domain.Logging;
using Talk.EventSourcing;
using static System.String;
using static Talk.Messages.Vehicle.Events;

namespace Talk.Domain.Vehicle
{
    public class VehicleState : AggregateState<VehicleState>
    {
        string CustomerId { get; set; }
        string Registration { get; set; }
        string State { get; set; }
        internal int MaxSpeed { get; private set; }
        internal int MaxTemperature { get; private set; }

        public override VehicleState When(VehicleState state, object @event)
        {
            return @event switch
            {
                VehicleRegistered e => With(state, x =>
                {
                    x.Id             = e.VehicleId;
                    x.CustomerId     = e.CustomerId;
                    x.State          = e.State;
                    x.Registration   = e.Registration;
                    x.MaxSpeed       = e.MaxSpeed;
                    x.MaxTemperature = e.MaxTemperature;
                }),
                VehicleMaxSpeedAdjusted e       => With(state, x => x.MaxSpeed = e.MaxSpeed),
                VehicleMaxTemperatureAdjusted e => With(state, x => x.MaxTemperature = e.MaxTemperature),
                VehicleOverheated e             => Overheated(e.Temperature),
                VehicleSpeeingDetected e        => Speeding(e.RecordedSpeed),
                _                               => state
            };

            VehicleState Overheated(int temperature)
            {
                Log.Warn($"Vehicle {state.Registration} is overheated. Temperature: {temperature}");
                return With(state, x => x.State = "Overheated");
            }

            VehicleState Speeding(int speed)
            {
                Log.Warn($"Vehicle {state.Registration} is speeding. Speed: {speed}");
                return With(state, x => x.State = "Speeding");
            }
        }

        public override string GetStreamName(string id)
            => $"Vehicle-{id}";

        protected override bool EnsureValidState(VehicleState newState)
            => newState switch
            {
                { } v when IsNullOrWhiteSpace(v.Id) || IsNullOrWhiteSpace(v.CustomerId) ||
                IsNullOrWhiteSpace(v.Registration) => false,
                _ => true
            };

        static readonly ILog Log = LogProvider.GetCurrentClassLogger();
    }
}