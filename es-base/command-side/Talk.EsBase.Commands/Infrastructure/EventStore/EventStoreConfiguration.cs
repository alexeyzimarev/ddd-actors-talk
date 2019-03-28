using EventStore.ClientAPI;

namespace Talk.EsBase.Commands.Infrastructure.EventStore
{
    public static class EventStoreConfiguration
    {
        public static IEventStoreConnection ConfigureEsConnection(
            string connectionString,
            string connectionName) =>
            EventStoreConnection.Create(
                connectionString,
                ConnectionSettings.Create().KeepReconnecting(),
                connectionName
            );
    }
}