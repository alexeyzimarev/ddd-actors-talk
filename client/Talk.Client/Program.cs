using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Talk.Client.Seeds;

namespace Talk.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            SetupThreadPool();

            // Include port of the gRPC server as an application argument
//            var port = args.Length > 0 ? args[0] : "50051";

//            var channel = new Channel("localhost:" + port, ChannelCredentials.Insecure);

            var bus = BusConfiguration.ConfigureMassTransit();
            await bus.StartAsync();

            await CustomerSeed.Publish(bus);

//            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            await bus.StopAsync();
        }

        static void SetupThreadPool()
        {
            ThreadPool.GetMaxThreads(out var workerThreads, out var completionPortThreads);
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            workerThreads = Math.Max(workerThreads, 100);
            completionPortThreads = Math.Max(completionPortThreads, 400);
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);

            ServicePointManager.DefaultConnectionLimit = 400;
            ServicePointManager.UseNagleAlgorithm = false;
        }
    }
}