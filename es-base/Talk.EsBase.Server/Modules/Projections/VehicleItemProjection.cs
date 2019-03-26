using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Talk.EsBase.Server.Infrastructure.RavenDb;
using static Talk.Domain.VehicleEvents;
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
                                    State = e.State
                                }
                            ),
                        _ => (Func<Task>) null
                };

            Task Update(
                Guid id,
                Action<VehicleItem> update)
                => session.UpdateItem(
                    GetDbId(id),
                    update
                );

            string GetDbId(Guid vehicleId)
                => VehicleItem.GetDatabaseId(vehicleId);
        }
    }
}