using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proto;
using Proto.Persistence;
using Talk.EventSourcing;
using Talk.Proto.Messages.Commands;

namespace Talk.Actors.Commands.Infrastructure.ProtoActor
{
    public abstract class ServiceActor<T> : IActor
        where T : AggregateState<T>, new()
    {
        protected T State { get; set; }

        protected ServiceActor(IEventStore store)
        {
            State = new T();

            When<Started>(ctx =>
            {
                _persistence = Persistence.WithEventSourcing(store, ctx.Self.Id, ApplyEvent);
                return _persistence.RecoverStateAsync();
            });
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (!_handlers.TryGetValue(context.Message.GetType(), out var handler))
                return;

            await handler(context);

            if (context.Sender != null)
                context.Respond(new Ack());
        }

        void ApplyEvent(Event @event)
            => State = State.When(State, @event.Data);

        async Task Apply(AggregateState<T>.Result result)
        {
            foreach (var @event in result.Events)
                await _persistence.PersistEventAsync(@event);
        }

        protected void When<TCommand>(Func<IContext, Task> handler)
            where TCommand : class
            => _handlers.Add(typeof(TCommand), handler);

        protected void When<TCommand>(Func<TCommand, Task> handler)
            where TCommand : class
            => _handlers.Add(
                typeof(TCommand),
                ctx => handler(ctx.Message as TCommand));

        protected void When<TCommand>(Func<TCommand, AggregateState<T>.Result> handler)
            where TCommand : class
            => _handlers.Add(
                typeof(TCommand),
                ctx => Apply(handler(ctx.Message as TCommand))
            );

        readonly Dictionary<Type, Func<IContext, Task>> _handlers =
            new Dictionary<Type, Func<IContext, Task>>();

        Persistence _persistence;
    }
}