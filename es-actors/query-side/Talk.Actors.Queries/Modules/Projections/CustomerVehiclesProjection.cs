using System.Collections.Generic;
using System.Threading.Tasks;
using Proto.Persistence;
using Talk.Actors.Queries.Infrastructure.ProtoActor;
using static Talk.Actors.Queries.Modules.Projections.ReadModels;
using static Talk.Messages.Customer.Events;

namespace Talk.Actors.Queries.Modules.Projections
{
    public class CustomerVehiclesProjection : ProjectionActor<CustomerVehicles>
    {
        public CustomerVehiclesProjection(ISnapshotStore store) : base(store)
        {
            When<CustomerRegistered>(
                e =>
                {
                    State =
                        new CustomerVehicles
                        {
                            Id = e.CustomerId,
                            DisplayName = e.DisplayName,
                            Vehicles = new List<CustomerVehicles.Vehicle>()
                        };
                    return Task.FromResult(true);
                }
            );

            When<Messages.Vehicle.Events.VehicleRegistered>(
                e =>
                {
                    State.Vehicles.Add(new CustomerVehicles.Vehicle
                    {
                        VehicleId = e.VehicleId,
                        Registration = e.Registration,
                        State = e.State
                    });
                    return Task.FromResult(true);
                }
            );
        }
    }
}