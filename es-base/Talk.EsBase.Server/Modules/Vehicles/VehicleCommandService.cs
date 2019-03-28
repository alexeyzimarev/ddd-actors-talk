using System.Threading.Tasks;
using Talk.Domain.Vehicle;
using Talk.EventSourcing;
using static Talk.Messages.Vehicle.Commands;

namespace Talk.EsBase.Server.Modules.Vehicles
{
    public class VehicleCommandService : CommandService<VehicleState>
    {
        public VehicleCommandService(IAggregateStore store)
            : base(store) { }

        public Task Handle(RegisterVehicle command)
            => Handle(
                command.VehicleId,
                state => Vehicle.Register(
                    command.VehicleId,
                    command.CustomerId,
                    command.MakeModel,
                    command.Registration,
                    command.MaxSpeed,
                    command.MaxTemperature
                )
            );

        public Task Handle(AdjustMaxSpeed command)
            => Handle(
                command.VehicleId,
                state => Vehicle.AdjustMaxSpeed(state, command.MaxSpeed)
            );

        public Task Handle(AdjustMaxTemperature command)
            => Handle(
                command.VehicleId,
                state => Vehicle.AdjustMaxTemperature(state, command.MaxTemperature)
            );
    }
}