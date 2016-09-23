using Microsoft.AzureCAT.Samples.LoggingCommon.Serilog.Aggregator.StatsAggregator.Graphite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.Logging.Serilog.Aggregator.Graphite
{
    public class GraphitePublisherTarget  
    {
        public GraphitePublisherTarget(string server, int port)
        {
            this._hostName = server;
            this._port = port;
        }

        private static IDictionary<string, Func<MetricEvent, object>> metricMap =
            new Dictionary<string, Func<MetricEvent, object>>()
        {
                { ".avg", (e) => (int)e.Average },
                { ".min", (e) => (int)e.Min },
                { ".max", (e) => (int)e.Max },
                { ".count", (e) => e.Count },
        };

        private readonly string _hostName;
        private readonly int _port;

        public async Task SendMetric(IEnumerable<MetricEvent> evts)
        {
            if (evts == null)
                return;
            var eventList = evts.ToArray();
            if (eventList.Length == 0)
                return;

            try
            {
                // Send each metric as a set of individual gauges or counters (separated by newlines)
                // metric_path value timestamp\n
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(_hostName, _port);
                    using (var stream = tcpClient.GetStream())
                    using (var sw = new StreamWriter(stream))
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var evt in evts)
                        {
                            foreach (var kv in metricMap)
                            {
                                sb.Append(evt.MetricName);
                                sb.Append(kv.Key);
                                sb.Append(' ');
                                sb.Append(metricMap[kv.Key](evt));
                                sb.Append(" ");
                                sb.Append(evt.Timestamp.ToUnixTimeSeconds());
                                await sw.WriteLineAsync(sb.ToString());
                                sb.Clear();
                            }                          
                        }                        
                    }
                }
            }
            catch (Exception ex0)
            {
                // In the case of failure log the error count and continue.  These are
                // continuous values, so the next value will replace anyway
              // TODO
            }
        }
    }
}
