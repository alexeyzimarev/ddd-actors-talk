using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Talk.Domain.Customer;
using Talk.EsBase.Server.Infrastructure.RavenDb;
using static Talk.EsBase.Server.Modules.Projections.ReadModels;

namespace Talk.EsBase.Server.Modules.Projections
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
                                new CustomerVehicles
                                {
                                    Id = e.CustomerId,
                                    DisplayName = e.DisplayName
                                }
                            ),
                    _ => (Func<Task>) null
                };

            Task Update(
                string id,
                Action<CustomerVehicles> update)
                => session.UpdateItem(
                    GetDbId(id),
                    update
                );

            string GetDbId(string vehicleId)
                => CustomerVehicles.GetDatabaseId(vehicleId);
        }
    }
}