using System.Threading.Tasks;
using Talk.Domain.Vehicle;
using Talk.EventSourcing;
using static Talk.Messages.Vehicle.Commands;

namespace Talk.Actors.Commands.Modules.Vehicles
{
    public class VehicleCommandService : CommandService<VehicleState>
    {
        public VehicleCommandService(IAggregateStore store)
            : base(store)
        {
            When<RegisterVehicle>(
                cmd => cmd.VehicleId,
                (state, cmd) => Vehicle.Register(
                    cmd.VehicleId,
                    cmd.CustomerId,
                    cmd.MakeModel,
                    cmd.Registration,
                    cmd.MaxSpeed,
                    cmd.MaxTemperature
                )
            );

            When<AdjustMaxSpeed>(
                cmd => cmd.VehicleId,
                (state, cmd) => Vehicle.AdjustMaxSpeed(state, cmd.MaxSpeed)
            );

            When<AdjustMaxTemperature>(
                cmd => cmd.VehicleId,
                (state, cmd) => Vehicle.AdjustMaxTemperature(state, cmd.MaxTemperature)
            );

            When<RegisterVehicleTelemetry>(
                cmd => cmd.VehicleId,
                (state, cmd) => Vehicle.ProcessTelemetry(state, cmd.Speed, cmd.Temperature)
            );
        }
    }
}