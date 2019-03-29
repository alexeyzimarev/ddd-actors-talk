using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proto;
using Proto.Persistence;
using Serilog;
using Talk.Proto.Messages.Events;

namespace Talk.Actors.Queries.Infrastructure.ProtoActor
{
    public abstract class ProjectionActor<T> : IActor where T : new()
    {
        protected T State { get; set; }
        protected ILogger Log;

        protected ProjectionActor(ISnapshotStore store)
        {
            Log = Serilog.Log.ForContext(GetType());

            State = new T();

            When<Started>(async ctx =>
            {
                _persistence = Persistence.WithSnapshotting(
                    store, ctx.Self.Id, ApplySnapshot
                    );
                await _persistence.RecoverStateAsync();
                return false;
            });
        }

        void ApplySnapshot(Snapshot snapshot)
        {
            if (!(snapshot is RecoverSnapshot message)) return;

            if (message.State is T state)
            {
                State = state;
            }
            else
            {
                Log.Error("Wrong snapshot type");
            }
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (!_handlers.TryGetValue(context.Message.GetType(), out var handler))
                return;

            var changed = await handler(context);

            if (changed)
                await _persistence.PersistSnapshotAsync(State);

            if (context.Sender != null)
                context.Respond(new AckEvent());
        }

        protected void When<TEvent>(Func<IContext, Task<bool>> handler)
            where TEvent : class
            => _handlers.Add(typeof(TEvent), handler);

        protected void When<TEvent>(Func<TEvent, Task<bool>> handler)
            where TEvent : class
            => _handlers.Add(
                typeof(TEvent),
                ctx => handler(ctx.Message as TEvent));

        readonly Dictionary<Type, Func<IContext, Task<bool>>> _handlers =
            new Dictionary<Type, Func<IContext, Task<bool>>>();

        Persistence _persistence;
    }
}