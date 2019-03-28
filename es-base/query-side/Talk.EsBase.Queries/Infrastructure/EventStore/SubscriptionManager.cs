using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Serilog;
using Serilog.Events;
using Talk.EsBase.Queries.Infrastructure.Prometheus;
using EventHandler = Talk.EventSourcing.EventHandler;
using ILogger = Serilog.ILogger;

namespace Talk.EsBase.Queries.Infrastructure.EventStore
{
    public class SubscriptionManager
    {
        static readonly ILogger _log =Log.ForContext<SubscriptionManager>();

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
        }

        public async Task Start()
        {
            var settings = new CatchUpSubscriptionSettings(
                2000, 500,
                _log.IsEnabled(LogEventLevel.Debug),
                false
            );

            _log.Debug("Starting the subscription manager...");

            var position = await _checkpointStore.GetCheckpoint();
            _log.Debug("Retrieved the checkpoint: {checkpoint}", position);

            _subscription = _connection.SubscribeToAllFrom(position,
                settings, EventAppeared, LiveProcessingStarted, SubscriptionDropped
            );

            _log.Debug("Subscribed to $all stream");
        }

        void SubscriptionDropped(
            EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason reason, Exception exception)
        {
            _log.Error(
                exception,
                "Subscription {subscription} dropped because of {reason}",
                _subscriptionName,
                reason);
        }

        void LiveProcessingStarted(EventStoreCatchUpSubscription obj)
            => _log.Information("Live processing started for {subscription}", _subscriptionName);

        async Task EventAppeared(
            EventStoreCatchUpSubscription _,
            ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventType.StartsWith("$")) return;

            var @event = resolvedEvent.Deserialze();

            _log.Debug("Processing event {event}", @event.ToString());

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
                _log.Error(
                    e,
                    "Error occured when projecting the event {event}",
                    @event
                );
                throw;
            }
        }
    }
}