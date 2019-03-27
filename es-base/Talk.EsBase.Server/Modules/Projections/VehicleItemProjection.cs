using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Talk.EsBase.Server.Infrastructure.RavenDb;
using static Talk.Domain.Vehicle.Events;
using static Talk.EsBase.Server.Modules.Projections.ReadModels;

namespace Talk.EsBase.Server.Modules.Projections
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
                                new VehicleItem
                                {
                                    VehicleId = GetDbId(e.VehicleId),
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
                Action<VehicleItem> update)
                => session.UpdateItem(
                    GetDbId(id),
                    update
                );

            string GetDbId(string vehicleId)
                => VehicleItem.GetDatabaseId(vehicleId);
        }
    }
}