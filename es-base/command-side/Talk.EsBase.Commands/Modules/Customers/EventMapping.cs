using Talk.EventStore;
using Talk.Messages.Customer;

namespace Talk.EsBase.Commands.Modules.Customers
{
    public static class EventMapping
    {
        public static void Map()
            => TypeMapper
                .Map<Events.CustomerRegistered>("CustomerRegistered");
    }
}