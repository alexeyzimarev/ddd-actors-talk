using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Proto.Persistence;
using Talk.EventSourcing;

namespace Talk.Actors.Commands.Infrastructure.Prometheus
{
    public class MeasuredStore : IAggregateStore
    {
        readonly IAggregateStore _store;

        public MeasuredStore(IAggregateStore store) => _store = store;

        public Task Save<T>(long version, AggregateState<T>.Result update)
            where T : class, IAggregateState<T>, new()
            =>
                PrometheusMetrics.Measure(
                    () => _store.Save(version, update),
                    PrometheusMetrics.PersistenceTimer("save"),
                    PrometheusMetrics.PersistenceErrorCounter("save"));

        public Task<T> Load<T>(string id, Func<T, object, T> when)
            where T : IAggregateState<T>, new()
        {
            return PrometheusMetrics.Measure(
                () => _store.Load(id, when),
                PrometheusMetrics.PersistenceTimer("load"),
                PrometheusMetrics.PersistenceErrorCounter("load")
            );
        }

        public Task<bool> Exists(string streamName)
        {
            throw new NotImplementedException();
        }
    }

    public class MeasuredActorStore : IEventStore
    {
        readonly IEventStore _store;

        public MeasuredActorStore(IEventStore store) => _store = store;

        public Task<long> GetEventsAsync(
            string actorName,
            long indexStart,
            long indexEnd,
            Action<object> callback)
            => PrometheusMetrics.Measure(
                () => _store.GetEventsAsync(actorName, indexStart, indexEnd, callback),
                PrometheusMetrics.PersistenceTimer("load")
            );

        public Task<long> PersistEventAsync(
            string actorName,
            long index,
            object @event)
            => PrometheusMetrics.Measure(
                () => _store.PersistEventAsync(actorName, index, @event),
                PrometheusMetrics.PersistenceTimer("save")
            );

        public Task<long> PersistEventsAsync(string actorName, IEnumerable<PersistedEvent> @event)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEventsAsync(string actorName, long inclusiveToIndex)
        {
            throw new NotImplementedException();
        }
    }
}