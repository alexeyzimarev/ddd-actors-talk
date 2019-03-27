using System;
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

        protected async Task Handle(
            string id,
            Func<T, AggregateState<T>.Result> update)
        {
            try
            {
                var state = await Load(id);
                await _store.Save(state.Version, update(state));
            }
            catch (Exception e)
            {
                _log.Error(e, "Error occured while handling the command");
                throw;
            }
        }
    }
}