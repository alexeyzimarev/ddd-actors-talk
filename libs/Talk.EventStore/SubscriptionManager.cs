using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Serilog;
using Serilog.Events;
using EventHandler = Talk.EventSourcing.EventHandler;
using ILogger = Serilog.ILogger;

namespace Talk.EventStore
{
    public class SubscriptionManager
    {
        static readonly ILogger _log = Log.ForContext<SubscriptionManager>();

        readonly ICheckpointStore        _checkpointStore;
        readonly string                  _subscriptionName;
        readonly IEventStoreConnection   _connection;
        readonly EventHandler[]          _eventHandlers;
        EventStoreAllCatchUpSubscription _subscription;

        public SubscriptionManager(
            IEventStoreConnection connection,
            ICheckpointStore checkpointStore,
            string subscriptionName,
            params EventHandler[] eventHandlers
        )
        {
            _connection       = connection;
            _checkpointStore  = checkpointStore;
            _subscriptionName = subscriptionName;
            _eventHandlers    = eventHandlers;
        }

        public async Task Start()
        {
            var settings = new CatchUpSubscriptionSettings(
                2000, 500,
                _log.IsEnabled(LogEventLevel.Debug),
                false
            );

            _log.Information("Starting the subscription manager...");

            var position = await _checkpointStore.GetCheckpoint();
            _log.Information("Retrieved the checkpoint: {checkpoint}", position);

            _subscription = _connection.SubscribeToAllFrom(position,
                settings, EventAppeared, LiveProcessingStarted, SubscriptionDropped
            );

            _log.Information("Subscribed to $all stream");
        }

        void SubscriptionDropped(
            EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason reason, Exception exception
        )
        {
            _log.Warning(
                exception,
                "Subscription {subscription} dropped because of {reason}",
                _subscriptionName,
                reason);
            if (reason != SubscriptionDropReason.UserInitiated)
                Task.Run(Start);
        }

        void LiveProcessingStarted(EventStoreCatchUpSubscription obj)
            => _log.Information("Live processing started for {subscription}", _subscriptionName);

        async Task EventAppeared(
            EventStoreCatchUpSubscription _,
            ResolvedEvent resolvedEvent
        )
        {
            if (resolvedEvent.Event.EventType.StartsWith("$")) return;

            var @event = resolvedEvent.Deserialze();

            _log.Debug("Projecting event {event}", @event.ToString());

            try
            {
                await Task.WhenAll(_eventHandlers.Select(x => x(@event)));

                await _checkpointStore.StoreCheckpoint(resolvedEvent.OriginalPosition);
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