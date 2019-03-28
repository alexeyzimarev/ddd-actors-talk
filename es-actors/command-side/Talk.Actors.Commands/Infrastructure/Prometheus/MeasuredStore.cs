using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
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
}