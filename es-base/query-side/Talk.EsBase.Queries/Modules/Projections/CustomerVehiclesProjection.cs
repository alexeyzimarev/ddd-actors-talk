using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Talk.Domain.Customer;
using Talk.EsBase.Queries.Infrastructure.RavenDb;
using Talk.Messages.Customer;
using static Talk.EsBase.Queries.Modules.Projections.ReadModels;

namespace Talk.EsBase.Queries.Modules.Projections
{
    public static class CustomerVehiclesProjection
    {
        public static Func<Task> GetHandler(
            IAsyncDocumentSession session,
            object @event)
        {
            return @event switch
                {
                    Events.CustomerRegistered e =>
                        () =>
                            session.StoreAsync(
                                new ReadModels.CustomerVehicles
                                {
                                    Id = GetDbId(e.CustomerId),
                                    DisplayName = e.DisplayName,
                                    Vehicles = new List<ReadModels.CustomerVehicles.Vehicle>()
                                }
                            ),
                    Messages.Vehicle.Events.VehicleRegistered e =>
                        () =>
                            Update(
                                e.CustomerId,
                                c => c.Vehicles.Add(
                                    new ReadModels.CustomerVehicles.Vehicle
                                    {
                                        VehicleId = e.VehicleId,
                                        Registration = e.Registration,
                                        State = e.State
                                    })
                            ),
                    _ => (Func<Task>) null
                };

            Task Update(
                string id,
                Action<ReadModels.CustomerVehicles> update)
                => session.UpdateItem(
                    GetDbId(id),
                    update
                );

            string GetDbId(string vehicleId)
                => ReadModels.CustomerVehicles.GetDatabaseId(vehicleId);
        }
    }
}