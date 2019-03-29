using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Proto;
using Proto.Cluster;
using Proto.Remote;
using Talk.Messages.Vehicle;
using Talk.Proto.Messages;
using Talk.Proto.Messages.Commands;

namespace Talk.Proto.Client.Seeds
{
    public static class VehicleSeed
    {
        public static Task<IEnumerable<Vehicle>> BusSeed(IPublishEndpoint bus)
            => Seed(
                v =>
                    new Commands.RegisterVehicle
                    {
                        CustomerId = v.CustomerId,
                        VehicleId = v.VehicleId,
                        Registration = v.Registration,
                        MaxSpeed = 100,
                        MaxTemperature = 100
                    },
                cmd => bus.Publish(cmd));

        public static Task<IEnumerable<Vehicle>> ProtoSeed()
            => Seed(
                v =>
                    new RegisterVehicle
                    {
                        CustomerId = v.CustomerId,
                        VehicleId = v.VehicleId,
                        Registration = v.Registration,
                        MaxSpeed = 100,
                        MaxTemperature = 100
                    },
                async cmd =>
                {
                    var actor = await ProtoCluster.GetActor("Vehicle", cmd.VehicleId);
                    var response = await RootContext.Empty.RequestAsync<Ack>(actor, cmd);
                }
            );

        public static async Task<IEnumerable<Vehicle>> Seed<T>(
            Func<Vehicle, T> commandFactory,
            Func<T, Task> publisher)
        {
            var vehicles = GenerateVehicles(1, 100, 30).ToList();
            await Task.WhenAll(
                vehicles.Select(v => publisher(commandFactory(v)))
            );
            return vehicles;
        }

        public class Vehicle
        {
            public string CustomerId { get; set; }
            public string VehicleId { get; set; }
            public string Registration { get; set; }
        }

        public static IEnumerable<Vehicle> GenerateVehicles(
            int startCustomerId,
            int customersCount,
            int vehiclesPerCustomer) =>
            Enumerable
                .Range(startCustomerId, customersCount)
                .Select(
                    x =>
                        new
                        {
                            CustomerId = x,
                            VehicleIds = Enumerable.Range(1, vehiclesPerCustomer)
                        })
                .SelectMany(
                    x => x.VehicleIds.Select(
                        v => new Vehicle
                        {
                            CustomerId = x.CustomerId.ToString(),
                            VehicleId = x.CustomerId * 1000 + v.ToString(),
                            Registration = $"{x.CustomerId:0000}-{v:0000}"
                        })
                );
    }
}