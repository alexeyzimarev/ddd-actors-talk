using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Talk.EsBase.Commands.Infrastructure.EventStore
{
    public class EventStoreService : IHostedService
    {
        readonly IEventStoreConnection _esConnection;
        readonly ConnectionSupervisor _supervisor;

        public EventStoreService(
            IEventStoreConnection esConnection)
        {
            _esConnection = esConnection;
            _supervisor = new ConnectionSupervisor(
                esConnection,
                () => Log.Fatal("Fatal failure with EventStore connection"));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _supervisor.Initialize();
            await _esConnection.ConnectAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _supervisor.Shutdown();
            _esConnection.Close();
            return Task.CompletedTask;
        }
    }
}