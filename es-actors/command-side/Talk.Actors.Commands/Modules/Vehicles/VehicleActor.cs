using Proto.Persistence;
using Talk.Actors.Commands.Infrastructure.ProtoActor;
using Talk.Domain.Vehicle;
using Talk.Proto.Messages.Commands;

namespace Talk.Actors.Commands.Modules.Vehicles
{
    public class VehicleActor : ServiceActor<VehicleState>
    {
        public VehicleActor(IEventStore provider) : base(provider)
        {
            When<RegisterVehicle>(
                cmd =>
                    Vehicle.Register(
                        cmd.VehicleId,
                        cmd.CustomerId,
                        cmd.MakeModel,
                        cmd.Registration,
                        cmd.MaxSpeed,
                        cmd.MaxTemperature)
            );

            When<RegisterVehicleTelemetry>(
                cmd =>
                    Vehicle.ProcessTelemetry(
                        State,
                        cmd.Speed,
                        cmd.Temperature)
            );
        }
    }
}