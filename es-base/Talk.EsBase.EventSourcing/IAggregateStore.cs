using System;
using System.Threading.Tasks;
using Marketplace.EventSourcing;

namespace Talk.EsBase.EventSourcing
{
    public interface IAggregateStore
    {
        Task Save<T>(
            long version,
            AggregateState<T>.Result update)
            where T : class, IAggregateState<T>, new();

        Task<T> Load<T>(Guid id, Func<T, object, T> when)
            where T : IAggregateState<T>, new();

        Task<bool> Exists(string streamName);
    }
}