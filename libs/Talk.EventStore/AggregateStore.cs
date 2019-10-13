using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Serilog;
using Talk.EventSourcing;
using ILogger = Serilog.ILogger;

namespace Talk.EventStore
{
    public class AggregateStore : IAggregateStore
    {
        readonly IEventStoreConnection _connection;
        static readonly ILogger _log = Log.ForContext<AggregateStore>();

        public AggregateStore(IEventStoreConnection connection)
            => _connection = connection;

        public Task Save<T>(
            long version,
            AggregateState<T>.Result update)
            where T : class, IAggregateState<T>, new()
        {
            var toSave = update.Events.ToArray();
            foreach (var @event in toSave)
                _log.Debug("Persisting event {event}", @event);

            return _connection.AppendEvents(
                update.State.StreamName, version, toSave
            );
        }

        public async Task<T> Load<T>(string id, Func<T, object, T> when)
            where T : IAggregateState<T>, new()
        {
            var state = new T();
            var streamName = state.GetStreamName(id);

            var events = await _connection.ReadEvents(streamName);

            return events.Select(x => x.Deserialze())
                .Aggregate(state, when);

        }

        public async Task<bool> Exists(string streamName)
        {
            var result = await _connection.ReadEventAsync(streamName, 1, false);
            return result.Status != EventReadStatus.NoStream;
        }
    }
}