using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Talk.EsBase.Queries.Infrastructure.RavenDb;
using static Talk.Messages.Vehicle.Events;
using static Talk.EsBase.Queries.Modules.Projections.ReadModels;

namespace Talk.EsBase.Queries.Modules.Projections
{
    public static class VehicleItemProjection
    {
        public static Func<Task> GetHandler(
            IAsyncDocumentSession session,
            object @event)
        {
            return @event switch
                {
                    VehicleRegistered e =>
                        () =>
                            session.StoreAsync(
                                new ReadModels.VehicleItem
                                {
                                    Id = GetDbId(e.VehicleId),
                                    Registration = e.Registration,
                                    MakeModel = e.MakeModel,
                                    MaxSpeed = e.MaxSpeed,
                                    MaxTemperature = e.MaxTemperature,
                                    State = e.State
                                }
                            ),
                    VehicleMaxSpeedAdjusted e =>
                        () => Update(e.VehicleId, x => x.MaxSpeed = e.MaxSpeed),
                    VehicleMaxTemperatureAdjusted e =>
                        () => Update(e.VehicleId, x => x.MaxTemperature = e.MaxTemperature),
                        _ => (Func<Task>) null
                };

            Task Update(
                string id,
                Action<ReadModels.VehicleItem> update)
                => session.UpdateItem(
                    GetDbId(id),
                    update
                );

            string GetDbId(string vehicleId)
                => ReadModels.VehicleItem.GetDatabaseId(vehicleId);
        }
    }
}