using System;
using System.Threading.Tasks;
using Marketplace.EventSourcing;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents.Session;
using Talk.EsBase.EventSourcing;
using Talk.EsBase.Server.Infrastructure.Logging;

namespace Talk.EsBase.Server.Infrastructure.RavenDb
{
    public class RavenDbProjection<T> : IProjection
    {
        static readonly string ReadModelName = typeof(T).Name;

        public RavenDbProjection(
            Func<IAsyncDocumentSession> getSession,
            Projector projector)
        {
            _projector = projector;
            GetSession = getSession;
            _log = Logger.ForContext(GetType());
        }

        Func<IAsyncDocumentSession> GetSession { get; }
        readonly Projector _projector;
        readonly ILogger _log;

        public async Task Project(object @event)
        {
            using (var session = GetSession())
            {
                var handler = _projector(session, @event);

                if (handler != null)
                {
                    _log.LogDebug(
                        "Projecting {event} to {model}",
                        @event,
                        ReadModelName
                    );
                    
                    await handler();
                    await session.SaveChangesAsync();
                }
            }
        }

        public delegate Func<Task> Projector(
            IAsyncDocumentSession session,
            object @event);
    }
}