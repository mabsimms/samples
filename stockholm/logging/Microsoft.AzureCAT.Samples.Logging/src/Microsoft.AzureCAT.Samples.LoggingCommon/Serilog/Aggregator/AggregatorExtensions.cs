using Microsoft.AzureCAT.Samples.Logging.Serilog.Aggregator;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.LoggingCommon.Serilog.Aggregator
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration StatsAggregator(
             this LoggerSinkConfiguration loggerConfiguration,
             IConfiguration config)
        {          
            var sink = new SerilogMetricAggregator(config);
            return loggerConfiguration.Sink(sink);
        }
    }
}
