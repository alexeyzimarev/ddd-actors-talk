using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace Talk.Actors.Queries.Infrastructure.RavenDb
{
    public static class RavenDbConfiguration
    {
        public static IDocumentStore ConfigureRavenDb(
            string serverUrl,
            string database)
        {
            var store = new DocumentStore
            {
                Urls = new[] {serverUrl},
                Database = database
            };
            store.Initialize();

            var record = store.Maintenance.Server.Send(
                new GetDatabaseRecordOperation(store.Database)
            );

            if (record == null)
                store.Maintenance.Server.Send(
                    new CreateDatabaseOperation(
                        new DatabaseRecord(store.Database)
                    )
                );

            return store;
        }
    }
}