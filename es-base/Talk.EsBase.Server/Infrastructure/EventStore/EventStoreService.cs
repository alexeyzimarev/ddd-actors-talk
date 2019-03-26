using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;

namespace Talk.EsBase.Server.Infrastructure.EventStore
{
    public class EventStoreService : IHostedService
    {
        readonly IEventStoreConnection _esConnection;
        readonly SubscriptionManager[] _subscriptionManager;

        public EventStoreService(
            IEventStoreConnection esConnection,
            params SubscriptionManager[] subscriptionManagers)
        {
            _esConnection = esConnection;
            _subscriptionManager = subscriptionManagers;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _esConnection.ConnectAsync();

            await Task.WhenAll(
                _subscriptionManager
                    .Select(projection => projection.Start())
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _esConnection.Close();
            return Task.CompletedTask;
        }
    }
}