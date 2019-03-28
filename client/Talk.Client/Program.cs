using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Talk.Client.Seeds;

namespace Talk.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            SetupThreadPool();

            var bus = BusConfiguration.ConfigureMassTransit();
            await bus.StartAsync();

//            await CustomerSeed.Publish(bus);

            await VehicleSeed.Publish(bus);

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