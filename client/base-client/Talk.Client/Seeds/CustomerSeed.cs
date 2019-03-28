using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Talk.Messages.Customer;

namespace Talk.Client.Seeds
{
    public static class CustomerSeed
    {
        public static Task Publish(IPublishEndpoint bus) =>
            Task.WhenAll(Enumerable.Range(1, 100).Select(id =>
                bus.Publish(
                    new Commands.RegisterCustomer
                    {
                        CustomerId = id.ToString(),
                        DisplayName = $"Customer {id}"
                    })
            ));
    }
}