using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Proto;
using Proto.Cluster;
using Proto.Remote;
using Talk.Messages.Vehicle;
using Talk.Proto.Messages;

namespace Talk.Proto.Client.Seeds
{
    public static class VehicleSeed
    {
        public static Task BusSeed(IPublishEndpoint bus)
            => Seed(
                (cId, vId) =>
                    new Commands.RegisterVehicle
                    {
                        CustomerId = cId.ToString(),
                        VehicleId = cId * 1000 + vId.ToString(),
                        Registration = $"{cId:0000}-{vId:0000}",
                        MaxSpeed = 100,
                        MaxTemperature = 100
                    },
                cmd => bus.Publish(cmd));

        public static Task ProtoSeed()
            => Seed(
                (cId, vId) =>
                    new RegisterVehicle
                    {
                        CustomerId = cId.ToString(),
                        VehicleId = cId * 1000 + vId.ToString(),
                        Registration = $"{cId:0000}-{vId:0000}",
                        MaxSpeed = 100,
                        MaxTemperature = 100
                    },
                async cmd =>
                {
                    var actor = await ProtoCluster.GetActor("Vehicle", "Vehicle", cmd.VehicleId);
                    var response = await RootContext.Empty.RequestAsync<Ack>(actor, cmd);
                }
            );

        public static Task Seed<T>(
            Func<int, int, T> commandFactory,
            Func<T, Task> publisher) =>
            Task.WhenAll(
                Enumerable
                    .Range(1, 100)
                    .Select(
                        x =>
                            new
                            {
                                CustomerId = x,
                                VehicleIds = Enumerable.Range(1, 30)
                            })
                    .SelectMany(
                        x => x.VehicleIds.Select(v => publisher(commandFactory(x.CustomerId, v)))
                    )
            );
    }
}