using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Talk.EsBase.EventSourcing;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Talk.EsBase.Server.Infrastructure.EventStore
{
    public class ProjectionManager
    {
        readonly ILogger _log;

        readonly ICheckpointStore _checkpointStore;
        readonly IEventStoreConnection _connection;
        readonly IProjection[] _projections;
        EventStoreAllCatchUpSubscription _subscription;

        public ProjectionManager(
            IEventStoreConnection connection,
            ICheckpointStore checkpointStore,
            params IProjection[] projections)
        {
            _connection = connection;
            _checkpointStore = checkpointStore;
            _projections = projections;
        }

        public async Task Start()
        {
            var settings = new CatchUpSubscriptionSettings(
                2000, 500,
                _log.IsEnabled(LogLevel.Debug),
                false
            );

            _log.LogDebug("Starting the projection manager...");

            var position = await _checkpointStore.GetCheckpoint();
            _log.LogDebug("Retrieved the checkpoint: {checkpoint}", position);

            _subscription = _connection.SubscribeToAllFrom(position,
                settings, EventAppeared
            );

            _log.LogDebug("Subscribed to $all stream");
        }

        async Task EventAppeared(
            EventStoreCatchUpSubscription _,
            ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventType.StartsWith("$")) return;

            var @event = resolvedEvent.Deserialze();

            _log.LogDebug("Projecting event {event}", @event.ToString());

            try
            {
                await Task.WhenAll(_projections.Select(x => x.Project(@event)));

                await _checkpointStore.StoreCheckpoint(
                    resolvedEvent.OriginalPosition.Value
                );
            }
            catch (Exception e)
            {
                _log.LogError(
                    e,
                    "Error occured when projecting the event {event}",
                    @event
                );
                throw;
            }
        }
    }
}