using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyobi.Logging.Sinks.StatsAggregator
{
    public class GraphitePublisherConfiguration : IPublisherConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
    }
}
