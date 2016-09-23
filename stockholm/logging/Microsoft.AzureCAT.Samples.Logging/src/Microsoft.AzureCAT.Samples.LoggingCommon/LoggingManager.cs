using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Microsoft.AzureCAT.Samples.LoggingCommon
{
    public class LoggingManager
    {
        public static void Configure(IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            // Configure serilog for highly scalable structured logging
            // [note - in a true high scale production setting this would be ELK, not SEQ]
            var logConfig = new Serilog.LoggerConfiguration();

            // Add the enrichers for serilog context 
            logConfig = logConfig.Enrich.FromLogContext();

            // Add runtime enrichment properties (distributed systems are distributed; ensure 
            // that logs for this context are easily identifiable)
            // Set up the contextual variables
            var environmentName = config.GetValue<string>("logging:environment", "NoEnvironment"); 
            var instanceName = config.GetValue<string>("logging:instanceName", Dns.GetHostName());
            var appName = config.GetValue<string>("logging:appName", "UnknownAppName").Replace(".", "");

            logConfig = logConfig
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.WithProperty("Instance", instanceName)
                .Enrich.WithProperty("AppName", appName);

            // Set up the hot/cold sinks

            // If enabled, write raw logs to files (for later publishign to Azure Storage or some other
            // cheap long-term archival destination
            // TODO:
            // - write to event hub
            // - write to seq

            // TODO - add in-memory metric aggregator
            // TODO - write aggs to graphite

            //logConfig.WriteTo.RollingFile()
         
            
            // Configure the generic logger factory to use serilog
            loggerFactory.AddSerilog();

            var logger = loggerFactory.CreateLogger("fred");
            logger.LogInformation("liuke a {field}", "test");
        }
    }
}
