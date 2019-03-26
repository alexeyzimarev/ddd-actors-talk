using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Talk.EsBase.Server.Infrastructure.EventStore
{
    public static class EventStoreExtensions
    {
        public static Task AppendEvents(
            this IEventStoreConnection connection,
            string streamName,
            long version,
            params object[] events)
        {
            if (events == null || !events.Any()) return Task.CompletedTask;

            var preparedEvents = events
                .Select(
                    @event =>
                        new EventData(
                            Guid.NewGuid(),
                            TypeMapper.GetTypeName(@event.GetType()),
                            true,
                            Serialize(@event),
                            null
                        )
                )
                .ToArray();

            return connection.AppendToStreamAsync(
                streamName,
                version,
                preparedEvents
            );

            static byte[] Serialize(object data)
                => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        }

        public static async Task<List<ResolvedEvent>> ReadEvents(
            this IEventStoreConnection connection,
            string streamName
        )
        {
            var result = new List<ResolvedEvent>();
            var position = 0;
            const int sliceSize = 4096;
            while (true)
            {
                var slice = await connection.ReadStreamEventsForwardAsync(
                    streamName, position, sliceSize, false
                );
                result.AddRange(slice.Events);
                if (slice.IsEndOfStream) break;
                position += sliceSize;
            }

            return result;
        }
    }
}