using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.Logging
{
    internal class SerilogMetricsTransforms : IPipelineFunctions
    {
        // Filter out events that do not have this timing key
        public bool Filter(LogEvent evt)
        {
            if (!evt.Properties.ContainsKey("eventType"))
                return true;
            // TODO - fix the data formatting
            if (evt.Properties["eventType"].ToString() == "\"ActionTiming\"")
                return false;
            return true;           
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

        public string GetUnit(LogEvent evt)
        {
            if (evt.Properties.ContainsKey("elapsed"))
                return "ms";
            else
                return "unknown";
        }

        public decimal GetValue(LogEvent evt)
        {
            if (evt.Properties.ContainsKey("elapsed"))
                return (decimal)TimeSpan.Parse(evt.Properties["elapsed"].ToString()).TotalMilliseconds;
            return 0.0M;
        }

        private decimal ParseDecimal(LogEventPropertyValue logEventPropertyValue)
        {
            decimal res;
            var val = logEventPropertyValue.ToString();
            if (decimal.TryParse(val, out res))
                return res;
            return 0.0M;
        }
    }
}
