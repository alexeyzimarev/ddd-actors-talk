using System.Linq;
using System.Threading.Tasks;
using CustomersManagement;
using Grpc.Core;

namespace Talk.Client.Seeds
{
    public static class CustomerSeed
    {
        public static async Task Execute(Channel channel)
        {
            var client = new CustomerService.CustomerServiceClient(channel);

            for (var i = 1002; i < 2000; i++)
            {
                await Send(i);
            }

//            await Send(1001).ResponseAsync;

            AsyncUnaryCall<Ack> Send(int id)
                => client.RegisterAsync(
                    new RegisterCustomer
                    {
                        CustomerId = id.ToString(),
                        DisplayName = $"Customer {id}"
                    });
        }
    }
}