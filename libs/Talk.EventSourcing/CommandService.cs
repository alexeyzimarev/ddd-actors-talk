using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace Talk.EventSourcing
{
    public abstract class CommandService<T>
        where T : class, IAggregateState<T>, new()
    {
        readonly IAggregateStore _store;
        static ILogger _log;

        protected CommandService(IAggregateStore store)
        {
            _store = store;
            _log = Log.ForContext(GetType());
        }

        Task<T> Load(string id)
            => _store.Load<T>(
                id,
                (x, e) => x.When(x, e)
            );

        public Task Handle<TCommand>(TCommand command)
        {
            var handler = _handlers[typeof(TCommand)];
            return handler(command);
        }

        protected void When<TCommand>(
            Func<TCommand, string> getId,
            Func<T, TCommand, AggregateState<T>.Result> update)
            where TCommand : class => _handlers.Add(
            typeof(TCommand),
            cmd =>
            {
                var command = cmd as TCommand;
                return Handle(command, getId(command), update);
            });

        async Task Handle<TCommand>(
            TCommand command,
            string id,
            Func<T, TCommand, AggregateState<T>.Result> update)
        {
            try
            {
                _log.Debug("Processing command {command}", command);

                var state = await Load(id);
                await _store.Save(state.Version, update(state, command));
            }
            catch (Exception e)
            {
                _log.Error(e, "Error occured while handling the command");
                throw;
            }
        }

        readonly Dictionary<Type, Func<object, Task>> _handlers =
            new Dictionary<Type, Func<object, Task>>();
    }
}