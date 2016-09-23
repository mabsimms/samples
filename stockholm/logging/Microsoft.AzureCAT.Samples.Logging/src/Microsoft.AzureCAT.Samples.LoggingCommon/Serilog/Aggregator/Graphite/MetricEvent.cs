using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.LoggingCommon.Serilog.Aggregator.StatsAggregator.Graphite
{
    public class MetricEvent
    {
        public string MetricName { get; set; }
        public string MetricUnit { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public decimal Average { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Count { get; set; }
    }
}
