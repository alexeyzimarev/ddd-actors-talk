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
        int MaxSpeed { get; set; }
        int MaxTemperature { get; set; }

        public override VehicleState When(VehicleState state, object @event)
            => @event switch
                {
                    VehicleRegistered e =>
                        With(state, x =>
                        {
                            x.Id = e.VehicleId;
                            x.CustomerId = e.CustomerId;
                            x.State = "Just registered";
                            x.Registration = e.Registration;
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
                    VehicleState v when IsNullOrWhiteSpace(v.Id) ||
                                        IsNullOrWhiteSpace(v.CustomerId) ||
                                        IsNullOrWhiteSpace(v.Registration) => false,
                    _ => true
                };
    }
}