using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Talk.Client.Seeds
{
    public static class VehicleSeed
    {
        public static Task Publish(IPublishEndpoint bus) =>
            Task.WhenAll(
                Enumerable.Range(1, 1000)
                    .Select(x => new {CustomerId = x, VehicleIds = Enumerable.Range(1, 50)})
                    .SelectMany(x =>
                        x.VehicleIds.Select(v =>
                            bus.Publish(
                                new Messages.Vehicle.Commands.RegisterVehicle
                                {
                                    CustomerId     = x.CustomerId.ToString(),
                                    VehicleId      = x.CustomerId * 1000 + v.ToString(),
                                    Registration   = $"{x.CustomerId:0000}-{v:0000}",
                                    MaxSpeed       = 100,
                                    MaxTemperature = 100
                                })
                        )
                    )
            );
    }
}