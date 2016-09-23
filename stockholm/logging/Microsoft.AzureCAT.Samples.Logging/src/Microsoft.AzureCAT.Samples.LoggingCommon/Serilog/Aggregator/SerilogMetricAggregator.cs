
using Microsoft.AzureCAT.Samples.LoggingCommon.Serilog.Aggregator.StatsAggregator.Graphite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using System.Text;
using Serilog.Core;

namespace Microsoft.AzureCAT.Samples.Logging.Serilog.Aggregator
{
    public class SerilogMetricAggregator : ILogEventSink
    {
        private Aggregator.SlidingWindowBase<global::Serilog.Events.LogEvent, MetricEvent> _pipeline;

        public SerilogMetricAggregator(IConfiguration config)
        {
            var server = config.GetValue<string>("logging:graphite:server", "127.0.0.1");
            var port = config.GetValue<int>("logging:graphite:port", 2001);

            var graphiteServer = new Graphite.GraphitePublisherTarget(server, port); 

            var funcs = new SerilogMetricsTransforms();
            _pipeline = new SlidingWindowBase<LogEvent, MetricEvent>(config,
                funcs.Filter,
                funcs.GetName,
                Transform,                       // Transform function
                graphiteServer.SendMetric   // Publish function
                );
        }

        public string GetName(LogEvent evt)
        {
            if (evt.Properties.ContainsKey("categoryName") &&
                evt.Properties.ContainsKey("operationType"))
            {
                StringBuilder opName = new StringBuilder();
                if (evt.Properties.ContainsKey("service"))
                {
                    opName.Append(evt.Properties["service"].ToString());
                    opName.Append('.');
                }


                if (evt.Properties.ContainsKey("categoryName"))
                {
                    opName.Append(evt.Properties["categoryName"].ToString());
                    opName.Append('.');
                }

                opName.Append(evt.Properties["operationType"].ToString());
                opName.Append('.');

                if (evt.Properties.ContainsKey("environment"))
                {
                    opName.Append(evt.Properties["environment"].ToString());
                    opName.Append('.');
                }

                // todo
                if (evt.Properties.ContainsKey("Instance"))
                {
                    opName.Append(evt.Properties["Instance"].ToString());
                    opName.Append('.');
                }

                return opName.ToString()
                    .ToLower()
                    .Replace(':', '.')
                    .Replace('/', '.')
                    .Replace("\"", "")
                    .TrimEnd('.')
                    ;
            }
            else
                return "unknown";
        }

        public decimal GetValue(LogEvent evt)
        {
            if (evt.Properties.ContainsKey("elapsed"))
                return (decimal)TimeSpan.Parse(evt.Properties["elapsed"].ToString()).TotalMilliseconds;
            return 0.0M;
        }

        public IEnumerable<MetricEvent> Transform(IEnumerable<LogEvent> evts)
        {
            return evts
                .OfType<LogEvent>()
                .Where(e => e.Properties.ContainsKey("elapsed"))
                // TODO - ensure that this works with the standard hooks from ai
                .GroupBy(e => GetName(e))
                .Select(e => new MetricEvent()
                {
                    MetricName = e.Key,
                    MetricUnit = "ms",
                    Average = e.Average(t => GetValue(t)),
                    Count = e.Count(),
                    Min = e.Min(t => GetValue(t)),
                    Max = e.Max(t => GetValue(t)),
                    Timestamp = e.Min(t => t.Timestamp)

                    // TODO - percentiles
                });           
        }

        public DateTimeOffset GetTimestamp(LogEvent le)
        {
            return le.Timestamp;
        }

        public void Emit(LogEvent logEvent)
        {
            _pipeline.Process(logEvent);
        }
    }
}
