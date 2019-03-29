using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Prometheus;

namespace Talk.Actors.Queries.Infrastructure.Prometheus
{
    public static class PrometheusMetrics
    {
        public static IHistogram LeadTimer(string messageType, string metricSource)
            => _leadTimer.Labels(_appName, messageType, metricSource);

        public static IHistogram SubscriptionTimer(string subscriptionName)
            => _subscriptionTimer.Labels(_appName, subscriptionName);

        public static IHistogram PersistenceTimer(string operation)
            => _persistenceTimer.Labels(_appName, operation);

        public static ICounter PersistenceErrorCounter(string operation)
            => _persistenceErrorCounter.Labels(_appName, operation);

        internal static IHistogram ConsumeTimer(string actorType, string messageType)
            => _consumerTimer.Labels(_appName, actorType, messageType);

        internal static ICounter ErrorCounter(string actorType, string messageType)
            => _errorCounter.Labels(_appName, actorType, messageType);

        internal static ICounter ActorsCounter(string actorType)
            => _actorsCounter.Labels(_appName, actorType);

        internal static void TryConfigure(string appName)
        {
            if (_isConfigured) return;

            _appName = appName;

            var bounds = new[]
                {.002, .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10, 30, 60, 120, 180, 240, 300};

            _leadTimer = Metrics.CreateHistogram(
                "app_message_lead_time_seconds",
                "The time between when message is received on the edge and when it is consumed, in seconds.",
                new HistogramConfiguration
                {
                    Buckets = bounds,
                    LabelNames = new[] {"appname", "message_type", "metric_source"}
                });

            _subscriptionTimer = Metrics.CreateHistogram(
                "app_event_subscription_time_seconds",
                "The time to process one event in a subscription, in seconds.",
                new HistogramConfiguration
                {
                    Buckets = bounds,
                    LabelNames = new[] {"appname", "subscription_name"}
                });

            _persistenceTimer = Metrics.CreateHistogram(
                "app_event_persistence_time_seconds",
                "The time to load or save an aggregate, in seconds.",
                new HistogramConfiguration
                {
                    Buckets = bounds,
                    LabelNames = new[] {"appname", "operation"}
                });

            _persistenceErrorCounter = Metrics.CreateCounter(
                "app_persistence_errors_count",
                "The number of persistence failures.",
                "appname", "operation");

            _consumerTimer = Metrics.CreateHistogram(
                "app_message_processing_time_seconds",
                "The time to consume a message, in seconds.",
                new HistogramConfiguration
                {
                    Buckets = bounds,
                    LabelNames = new []{"appname", "actor", "message"}
                });

            _actorsCounter = Metrics.CreateCounter(
                "app_actors_count",
                "The number of running actors.",
                "appname", "actor");

            _errorCounter = Metrics.CreateCounter(
                "app_message_failures_count",
                "The number of message processing failures.",
                "appname", "actor", "message");
            _isConfigured = true;
        }

        public static void ObserveLeadTime(string messageType, DateTimeOffset from, string metricSource)
        {
            var time = (DateTimeOffset.UtcNow - from).TotalSeconds;
            LeadTimer(messageType, metricSource).Observe(time);
        }

        public static async Task Measure(Func<Task> action, IHistogram metric, ICounter errorCounter = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await action();
            }
            catch (Exception)
            {
                errorCounter?.Inc();
                throw;
            }
            finally
            {
                stopwatch.Stop();
                metric.Observe(stopwatch.ElapsedTicks / (double) Stopwatch.Frequency);
            }
        }

       public static async Task<T> Measure<T>(Func<Task<T>> action, IHistogram metric, ICounter errorCounter = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            T result;

            try
            {
                result = await action();
            }
            catch (Exception)
            {
                errorCounter?.Inc();
                throw;
            }
            finally
            {
                stopwatch.Stop();
                metric.Observe(stopwatch.ElapsedTicks / (double) Stopwatch.Frequency);
            }

            return result;
        }

        static bool _isConfigured;
        static string _appName;
        static Histogram _leadTimer;
        static Histogram _subscriptionTimer;
        static Histogram _persistenceTimer;
        static Counter _persistenceErrorCounter;
        static Histogram _consumerTimer;
        static Counter _actorsCounter;
        static Counter _errorCounter;
    }
}