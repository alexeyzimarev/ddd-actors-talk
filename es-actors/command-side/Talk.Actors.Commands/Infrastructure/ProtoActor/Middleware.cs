using System;
using System.Diagnostics;
using Proto;
using Talk.Actors.Commands.Infrastructure.Prometheus;

namespace Talk.Actors.Commands.Infrastructure.ProtoActor
{
    public static class Middleware
    {
        public static Receiver Metrics(Receiver next, string actorType) =>
            async (context, envelope) =>
            {
                switch (envelope.Message)
                {
                    case Started _:
                        PrometheusMetrics.ActorsCounter(actorType).Inc();
                        break;
                    case Stopped _:
                        PrometheusMetrics.ActorsCounter(actorType).Inc(-1D);
                        break;
                }

                var messageType = envelope.Message?.GetType()?.Name;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    await next(context, envelope);
                }
                catch (Exception)
                {
                    PrometheusMetrics.ErrorCounter(actorType, messageType).Inc();
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    PrometheusMetrics.ConsumeTimer(actorType, messageType)
                        .Observe(stopwatch.ElapsedTicks / (double) Stopwatch.Frequency);
                }
            };
    }
}