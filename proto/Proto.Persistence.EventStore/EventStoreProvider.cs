using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Proto.Persistence.EventStore
{
    public class EventStoreProvider : IProvider
    {
        private readonly IEventStoreConnection _connection;
        private readonly StreamNameStrategy _eventStreamNameStrategy;
        private readonly StreamNameStrategy _snapshotStreamNameStrategy;
        private Func<string, Type> _stringToType;
        private Func<Type, string> _typeToString;

        public EventStoreProvider(IEventStoreConnection connection)
        {
            _connection = connection;
            _eventStreamNameStrategy = DefaultStrategy.DefaultEventStreamNameStrategy;
            _snapshotStreamNameStrategy = DefaultStrategy.DefaultSnapshotStreamNameStrategy;

            _stringToType = Type.GetType;
            _typeToString = type => type.AssemblyQualifiedName;
        }

        public EventStoreProvider(IEventStoreConnection connection,
            StreamNameStrategy eventStreamNameStrategy,
            StreamNameStrategy snapshotStreamNameStrategy)
        {
            _connection = connection;
            _eventStreamNameStrategy = eventStreamNameStrategy;
            _snapshotStreamNameStrategy = snapshotStreamNameStrategy;
        }

        public EventStoreProvider WithTypeResolver(Func<Type, string> typeToString, Func<string, Type> stringToType)
        {
            _stringToType = stringToType;
            _typeToString = typeToString;
            return this;
        }

        public async Task<long> GetEventsAsync(string actorName, long indexStart, long indexEnd,
            Action<object> callback)
        {
            var count = indexEnd == long.MaxValue ? indexEnd - 1 : indexEnd - indexStart + 1;
            var start = indexStart;
            if (indexStart > 0)
            {
                start = indexStart - 1;
                count++;
            }

            var slice = await _connection.ReadEvents(_eventStreamNameStrategy(actorName), start, count, _stringToType);

            var events = slice.Events.ToList();
            if (start != indexStart && events.Count > 0)
                events.RemoveAt(0);

            foreach (var @event in events)
            {
                callback(@event);
            }

            return slice.Version;
        }

        public async Task<(object Snapshot, long Index)> GetSnapshotAsync(string actorName)
        {
            var @event = await _connection.ReadLastEvent(_snapshotStreamNameStrategy(actorName), _stringToType);

            return (@event.Event, @event.Version);
        }

        public Task<long> PersistEventAsync(string actorName, long index, object @event)
            => _connection.SaveEvent(_eventStreamNameStrategy(actorName), @event, index, index - 1, _typeToString);

        public Task<long> PersistEventsAsync(string actorName, IEnumerable<PersistedEvent> @event)
        {
            throw new NotImplementedException();
        }

        public Task PersistSnapshotAsync(string actorName, long index, object snapshot)
            => _connection.SaveEvent(_snapshotStreamNameStrategy(actorName), snapshot, index,
                ExpectedVersion.Any, _typeToString);

        public Task DeleteEventsAsync(string actorName, long inclusiveToIndex)
        {
            throw new NotSupportedException("Deleting events is not supported by EventStore");
        }

        public Task DeleteSnapshotsAsync(string actorName, long inclusiveToIndex)
        {
            throw new NotSupportedException("Deleting snapshots is not supported by EventStore");
        }
    }
}