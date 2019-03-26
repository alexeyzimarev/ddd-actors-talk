using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Prometheus;

namespace Talk.EsBase.Server.Infrastructure.Prometheus
{
    public static class PrometheusMetrics
    {
        public static IHistogram LeadTimer(string messageType, string metricSource)
            => _leadTimer.Labels(_appName, messageType, metricSource);

        public static IHistogram SubscriptionTimer(string subscriptionName)
            => _subscriptionTimer.Labels(_appName, subscriptionName);

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


        static bool _isConfigured;
        static string _appName;
        static Histogram _leadTimer;
        static Histogram _subscriptionTimer;
    }
}