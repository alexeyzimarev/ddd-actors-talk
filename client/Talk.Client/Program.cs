using System;
using System.Threading.Tasks;
using Grpc.Core;
using Talk.Client.Seeds;

namespace Talk.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Include port of the gRPC server as an application argument
            var port = args.Length > 0 ? args[0] : "50051";

            var channel = new Channel("localhost:" + port, ChannelCredentials.Insecure);

            await CustomerSeed.Execute(channel);

            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}