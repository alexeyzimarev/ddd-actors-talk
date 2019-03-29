using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Proto;
using Log = Serilog.Log;

namespace Talk.Actors.Queries.Infrastructure.EventStore
{
    public class EventStoreService : IHostedService
    {
        readonly IEventStoreConnection _esConnection;
        readonly ConnectionSupervisor _supervisor;
        readonly Props _subscriptionManager;
        PID _pid;

        public EventStoreService(
            IEventStoreConnection esConnection,
            Props subscriptionManager)
        {
            _esConnection = esConnection;
            _subscriptionManager = subscriptionManager;
            _supervisor = new ConnectionSupervisor(
                esConnection,
                () => Log.Fatal("Fatal failure with EventStore connection"));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _supervisor.Initialize();
            await _esConnection.ConnectAsync();

            _pid = RootContext.Empty.Spawn(_subscriptionManager);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _pid.Poison();
            _supervisor.Shutdown();
            _esConnection.Close();
            return Task.CompletedTask;
        }
    }
}