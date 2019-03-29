using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Proto;
using Raven.Client.Documents.Linq;
using Serilog.Events;
using Talk.Actors.Queries.Infrastructure.Prometheus;
using Talk.Proto.Messages.Events;
using EventHandler = Talk.EventSourcing.EventHandler;
using ILogger = Serilog.ILogger;
using Log = Serilog.Log;

namespace Talk.Actors.Queries.Infrastructure.EventStore
{
    public class SubscriptionActor : IActor
    {
        static readonly ILogger _log = Log.ForContext<SubscriptionActor>();

        readonly ICheckpointStore _checkpointStore;
        readonly string _subscriptionName;
        readonly IEventStoreConnection _connection;
        readonly (Props, string)[] _projectionProps;
        EventStoreAllCatchUpSubscription _subscription;
        IEnumerable<PID> _projections;

        public SubscriptionActor(
            IEventStoreConnection connection,
            ICheckpointStore checkpointStore,
            string subscriptionName,
            params (Props, string)[] projectionProps)
        {
            _connection = connection;
            _checkpointStore = checkpointStore;
            _subscriptionName = subscriptionName;
            _projectionProps = projectionProps;
        }

        public async Task Start(IContext context)
        {
            var settings = new CatchUpSubscriptionSettings(
                2000, 500,
                _log.IsEnabled(LogEventLevel.Debug),
                false
            );

            _log.Information("Starting the subscription manager...");

            _projections = _projectionProps
                .Select(x => context.SpawnNamed(x.Item1, x.Item2));

            var position = await _checkpointStore.GetCheckpoint();
            _log.Information("Retrieved the checkpoint: {checkpoint}", position);

            _subscription = _connection.SubscribeToAllFrom(
                position,
                settings,
                (_, e) => EventAppeared(_, e, context.Self),
                LiveProcessingStarted,
                SubscriptionDropped
            );

            _log.Information("Subscribed to $all stream");
        }

        void SubscriptionDropped(
            EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason reason, Exception exception)
        {
            _log.Warning(
                exception,
                "Subscription {subscription} dropped because of {reason}",
                _subscriptionName,
                reason);
            if (reason != SubscriptionDropReason.UserInitiated)
                throw exception;
        }

        void LiveProcessingStarted(EventStoreCatchUpSubscription obj)
            => _log.Information("Live processing started for {subscription}", _subscriptionName);

        static Task EventAppeared(EventStoreCatchUpSubscription _,
            ResolvedEvent resolvedEvent, PID self)
        {
            if (resolvedEvent.Event.EventType.StartsWith("$"))
                return Task.CompletedTask;

            var @event = resolvedEvent.Deserialze();

            _log.Debug("Projecting event {event}", @event.ToString());

            RootContext.Empty.Send(
                self,
                new ProjectEvent
                {
                    Event = @event,
                    Position = resolvedEvent.OriginalPosition
                }
            );
            return Task.CompletedTask;
        }

        async Task Project(ISenderContext ctx, ProjectEvent @event)
        {
            try
            {
                await Task.WhenAll(
                    _projections
                        .Select(pid => ctx.RequestAsync<AckEvent>(pid, @event.Event)));

                await _checkpointStore.StoreCheckpoint(@event.Position.Value);
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

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    return Start(context);
                case ProjectEvent e:
                    return Project(context, e);
                default:
                    return Task.CompletedTask;
            }
        }
    }

    public class ProjectEvent
    {
        public Position? Position { get; set; }
        public object Event { get; set; }
    }
}