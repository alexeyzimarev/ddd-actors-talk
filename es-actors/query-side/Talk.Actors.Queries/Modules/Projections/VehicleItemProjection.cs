using System.Collections.Generic;
using System.Threading.Tasks;
using Proto.Persistence;
using Talk.Actors.Queries.Infrastructure.ProtoActor;
using static Talk.Messages.Vehicle.Events;
using static Talk.Actors.Queries.Modules.Projections.ReadModels;

namespace Talk.Actors.Queries.Modules.Projections
{
    public class VehicleItemProjection : ProjectionActor<VehicleItem>
    {
        public VehicleItemProjection(ISnapshotStore store) : base(store)
        {
            When<VehicleRegistered>(
                e =>
                {
                    State = new VehicleItem
                    {
                        Id = e.VehicleId,
                        Registration = e.Registration,
                        MakeModel = e.MakeModel,
                        MaxSpeed = e.MaxSpeed,
                        MaxTemperature = e.MaxTemperature,
                        State = e.State,
                        Sensors = new List<VehicleItem.VehicleSensor>()
                    };
                    return Task.FromResult(true);
                }
            );
        }
    }
}