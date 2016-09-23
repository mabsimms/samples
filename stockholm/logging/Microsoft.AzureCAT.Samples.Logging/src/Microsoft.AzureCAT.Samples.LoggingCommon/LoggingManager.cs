using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.AzureCAT.Samples.LoggingCommon.Serilog.Aggregator;

namespace Microsoft.AzureCAT.Samples.LoggingCommon
{
    public class LoggingManager
    {
        public static void Configure(IConfiguration config)
        {           
            // Configure serilog for highly scalable structured logging
            // [note - in a true high scale production setting this would be ELK, not SEQ]
            var logConfig = new global::Serilog.LoggerConfiguration()
                .MinimumLevel.Information();

            // Add the enrichers for serilog context 
            logConfig = logConfig.Enrich.FromLogContext();

            // - add enricher for log context
            // todo - need to update this one
            // logConfig = logConfig.Enrich.With<SerilogContextEnricher>();

            // Add enricher for environmental context
            logConfig = logConfig.Enrich.With(new SerilogEnvironmentEnricher(
                environmentName: config.GetValue<string>("logging:environment", "NoEnvironment"),
                region: config.GetValue<string>("logging:region", "no-region"),
                serviceName: config.GetValue<string>("logging:service", "no-service")
            ));

            ////////////////////////////////////////////////////////
            // Set up the hot/cold sinks

            // If enabled, write raw logs to files (for later publishign to Azure Storage or some other
            // cheap long-term archival destination
            // TODO:
            // - write to event hub
            // - write to seq
            logConfig = logConfig.WriteTo.Seq("http://localhost:5341/");


            // TODO - add in-memory metric aggregator
            logConfig.WriteTo.StatsAggregator(config);
            // TODO - write aggs to graphite

            // Set up the baseline logger
            Log.Logger = logConfig.CreateLogger();
               
        }
    }
}
