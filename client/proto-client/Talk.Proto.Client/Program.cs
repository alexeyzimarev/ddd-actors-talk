using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Consul;
using Proto.Remote;
using Talk.Client;
using Talk.Proto.Client.Seeds;
using Talk.Proto.Messages.Commands;

namespace Talk.Proto.Client
{
    public class Program
    {
        static IBusControl _bus;

        async static Task Main(string[] args)
        {
            SetupThreadPool();

            Log.SetLoggerFactory(ConfigureLogger());
            Serialization.RegisterFileDescriptor(CommandsReflection.Descriptor);

            _bus = BusConfiguration.ConfigureMassTransit();
            await _bus.StartAsync();
            Cluster.Start("talk-cluster", "localhost", 12999, GetProvider());

            Console.WriteLine("Press enter to seed customers");
            Console.ReadLine();

            await SeedCustomers();

            Console.WriteLine("Press enter to seed vehicles");
            Console.ReadLine();

            await SeedVehicles();

            Console.WriteLine("Done");
            Console.ReadLine();

            await _bus.StopAsync();
            Cluster.Shutdown();
        }

        static Task SeedVehicles() => VehicleSeed.ProtoSeed();

        static Task SeedCustomers() => CustomerSeed.Publish(_bus);

        static IClusterProvider GetProvider() =>
            new ConsulProvider(
                new ConsulProviderOptions
                {
                    DeregisterCritical = TimeSpan.FromSeconds(30),
                    RefreshTtl = TimeSpan.FromSeconds(2),
                    ServiceTtl = TimeSpan.FromSeconds(10),
                    BlockingWaitTime = TimeSpan.FromSeconds(20)
                },
                c => c.Address = new Uri("http://localhost:8500"));

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

        static ILoggerFactory ConfigureLogger()
        {
           var serviceProvider = new ServiceCollection()
                      .AddLogging(x => x.AddConsole())
                      .BuildServiceProvider();
           return serviceProvider.GetService<ILoggerFactory>();
        }
    }
}