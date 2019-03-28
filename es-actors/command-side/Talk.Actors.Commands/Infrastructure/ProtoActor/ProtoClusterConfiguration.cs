using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Consul;
using Proto.Persistence.EventStore;
using Proto.Remote;
using Talk.Actors.Commands.Infrastructure.EventStore;
using Talk.Actors.Commands.Modules.Vehicles;
using ILogger = Serilog.ILogger;
using ProtosReflection = Talk.Proto.Messages.ProtosReflection;

// ReSharper disable ConvertClosureToMethodGroup

namespace Talk.Actors.Commands.Infrastructure.ProtoActor
{
        public class ProtoClusterHostedService : IHostedService
        {
            static readonly ILogger Logger = Serilog.Log.ForContext<ProtoClusterHostedService>();

            readonly string _clusterName;
            readonly string _nodeAddress;
            readonly int _nodePort;
            readonly IEventStoreConnection _eventStoreConnection;
            readonly ConsulProvider _consulProvider;

            public ProtoClusterHostedService(
                Uri consulUrl,
                string clusterName,
                string nodeAddress,
                int nodePort,
                IEventStoreConnection eventStoreConnection
                )
            {
                _clusterName = clusterName;
                _nodeAddress = nodeAddress;
                _nodePort = nodePort;
                _eventStoreConnection = eventStoreConnection;
                _consulProvider = new ConsulProvider(
                    new ConsulProviderOptions
                    {
                        DeregisterCritical = TimeSpan.FromSeconds(30),
                        RefreshTtl = TimeSpan.FromSeconds(2),
                        ServiceTtl = TimeSpan.FromSeconds(10),
                        BlockingWaitTime = TimeSpan.FromSeconds(20)
                    },
                    c => c.Address = consulUrl);
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                Logger.Information($"Starting proto cluster node on {_nodeAddress}:{_nodePort}");

                Serialization.RegisterFileDescriptor(ProtosReflection.Descriptor);

                var eventStoreProvider = new EventStoreProvider(_eventStoreConnection)
                    .WithTypeResolver(
                        t => TypeMapper.GetTypeName(t),
                        s => Type.GetType(s));

                Remote.RegisterKnownKind("Vehicle",
                    Props.FromProducer(() => new VehicleActor(eventStoreProvider))
                        .WithReceiverMiddleware(next => Middleware.Metrics(next, "Vehicle")));

                Cluster.Start(_clusterName, _nodeAddress, _nodePort, _consulProvider);
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                Cluster.Shutdown();
                return Task.CompletedTask;
            }
        }
}