using Talk.Actors.Commands.Infrastructure.EventStore;
using Talk.Messages.Customer;

namespace Talk.Actors.Commands.Modules.Customers
{
    public static class EventMapping
    {
        public static void Map()
            => TypeMapper
                .Map<Events.CustomerRegistered>("CustomerRegistered");
    }
}