using System;
using System.Threading.Tasks;
using Talk.Client.Seeds;

namespace Talk.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var bus = BusConfiguration.ConfigureMassTransit();
            await bus.StartAsync();

            Console.WriteLine("Press enter to seed customers");
            Console.ReadLine();

            await CustomerSeed.Publish(bus);

            Console.WriteLine("Press enter to seed vehicles");
            Console.ReadLine();

            await VehicleSeed.Publish(bus);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            await bus.StopAsync();
        }
    }
}