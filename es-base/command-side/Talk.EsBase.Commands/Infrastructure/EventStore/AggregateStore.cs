using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Talk.EventSourcing;

namespace Talk.EsBase.Commands.Infrastructure.EventStore
{
    public class AggregateStore : IAggregateStore
    {
        readonly IEventStoreConnection _connection;

        public AggregateStore(IEventStoreConnection connection)
            => _connection = connection;

        public Task Save<T>(
            long version,
            AggregateState<T>.Result update)
            where T : class, IAggregateState<T>, new()
            => _connection.AppendEvents(
                update.State.StreamName, version, update.Events.ToArray()
            );

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