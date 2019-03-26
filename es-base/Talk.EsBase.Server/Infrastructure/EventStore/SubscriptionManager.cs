using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Talk.EsBase.Server.Infrastructure.Prometheus;
using EventHandler = Talk.EventSourcing.EventHandler;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Talk.EsBase.Server.Infrastructure.EventStore
{
    public class SubscriptionManager
    {
        readonly ILogger _log;

        readonly ICheckpointStore _checkpointStore;
        readonly string _subscriptionName;
        readonly IEventStoreConnection _connection;
        readonly EventHandler[] _eventHandlers;
        EventStoreAllCatchUpSubscription _subscription;

        public SubscriptionManager(
            IEventStoreConnection connection,
            ICheckpointStore checkpointStore,
            string subscriptionName,
            params EventHandler[] eventHandlers)
        {
            _connection = connection;
            _checkpointStore = checkpointStore;
            _subscriptionName = subscriptionName;
            _eventHandlers = eventHandlers;
            _log = Logging.Logger.ForContext<SubscriptionManager>();
        }

        public async Task Start()
        {
            var settings = new CatchUpSubscriptionSettings(
                2000, 500,
                _log.IsEnabled(LogLevel.Debug),
                false
            );

            _log.LogDebug("Starting the subscription manager...");

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

            _log.LogDebug("Processing event {event}", @event.ToString());

            try
            {
                await PrometheusMetrics.Measure(async () =>
                {
                    await Task.WhenAll(_eventHandlers.Select(x => x(@event)));

                    await _checkpointStore.StoreCheckpoint(
                        resolvedEvent.OriginalPosition.Value
                    );
                }, PrometheusMetrics.SubscriptionTimer(_subscriptionName));

                PrometheusMetrics.ObserveLeadTime(
                    resolvedEvent.Event.EventType,
                    resolvedEvent.Event.Created,
                    _subscriptionName);
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