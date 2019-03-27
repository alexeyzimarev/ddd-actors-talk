using System;
using System.Linq;
using System.Threading.Tasks;
using CustomersManagement;
using Grpc.Core;
using MassTransit;
using Talk.Messages.Customer;

namespace Talk.Client.Seeds
{
    public static class CustomerSeed
    {
        public static Task Publish(IPublishEndpoint bus) =>
            Task.WhenAll(Enumerable.Range(1000, 10000).Select(id =>
                bus.Publish(
                    new Commands.RegisterCustomer
                    {
                        CustomerId = id.ToString(),
                        DisplayName = $"Customer {id}"
                    })
            ));

        public static async Task Execute(Channel channel)
        {
            var client = new CustomerService.CustomerServiceClient(channel);

            for (var i = 0; i < 100; i++)
            {
                await Console.Out.WriteLineAsync($"Customers: {i}");
                await Task.WhenAll(
                    Enumerable
                        .Range(i * 100, 100)
                        .Select(id => RetryAck.Execute(
                            () =>
                                client.RegisterAsync(
                                    new RegisterCustomer
                                    {
                                        CustomerId = id.ToString(),
                                        DisplayName = $"Customer {id}"
                                    }).ResponseAsync
                        ))
                );
            }
        }
    }
}