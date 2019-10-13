using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Serilog;
using Talk.EventStore;

namespace Talk.EsBase.Commands.Infrastructure
{
    public class EventStoreService : IHostedService
    {
        readonly IEventStoreConnection _esConnection;
        readonly ConnectionSupervisor _supervisor;
        readonly SubscriptionManager[] _subscriptionManager;

        public EventStoreService(
            IEventStoreConnection esConnection,
            params SubscriptionManager[] subscriptionManagers)
        {
            _esConnection = esConnection;
            _subscriptionManager = subscriptionManagers;
            _supervisor = new ConnectionSupervisor(
                esConnection,
                () => Log.Fatal("Fatal failure with EventStore connection"));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _supervisor.Initialize();
            await _esConnection.ConnectAsync();

            await Task.WhenAll(
                _subscriptionManager
                    .Select(projection => projection.Start())
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _supervisor.Shutdown();
            _esConnection.Close();
            return Task.CompletedTask;
        }
    }
}