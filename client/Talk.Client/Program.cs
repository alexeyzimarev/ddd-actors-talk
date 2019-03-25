using System;
using System.Threading.Tasks;
using Grpc.Core;
using Processor;

namespace Talk.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Include port of the gRPC server as an application argument
            var port = args.Length > 0 ? args[0] : "50051";

            var channel = new Channel("localhost:" + port, ChannelCredentials.Insecure);
            var client = new EventReceiver.EventReceiverClient(channel);

            while (true)
            {
                var reply = await client.ReceiveEventAsync(new Telemetry {DeviceId = "GreeterClient"});
                Console.WriteLine("Greeting: " + reply.Message);
            }

            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}