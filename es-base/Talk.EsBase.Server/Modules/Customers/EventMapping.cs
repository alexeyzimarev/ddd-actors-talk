using Talk.Domain.Customer;
using Talk.EsBase.Server.Infrastructure.EventStore;
using Talk.Messages.Customer;

namespace Talk.EsBase.Server.Modules.Customers
{
    public static class EventMapping
    {
        public static void Map()
            => TypeMapper
                .Map<Events.CustomerRegistered>("CustomerRegistered");
    }
}