using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.DataPipelineRunner
{
    public class SampleDataGenerator : IDisposable
    {
      
        private readonly int _maxEventsPerTick;
        private System.Threading.Timer _eventTimer;
        private System.Threading.Timer _reportTimer;

        private System.Func<SampleData, bool> _publish;
        private long _eventsPublished = 0;
        private long _eventsDropped = 0;
        private readonly ILogger _logger;

        public SampleDataGenerator(int period, 
            int maxEventsPerTick,
            System.Func<SampleData, bool> pub,
            ILogger logger)
        {     
            _logger = logger;
            
            _eventTimer = new System.Threading.Timer(GenerateData, null, period, period);
            _reportTimer = new System.Threading.Timer(ReportData, null, 1000, 1000);
            _publish = pub;
            _maxEventsPerTick = maxEventsPerTick;

            _logger.LogInformation("SampleDataGenerator cycling at {period} with max of {maxEvents}",
                period, maxEventsPerTick);
        }

        private void GenerateData(object state)
        {
            var rand = new Random((int)Stopwatch.GetTimestamp());
            var evts = rand.Next(1, _maxEventsPerTick);        
            long published = 0;
            long dropped = 0;

            for (int i = 0; i < evts; i++)
            {
                var sd = new SampleData()
                {
                    Timestamp = DateTime.UtcNow,
                    SourceHost = "hostname",
                    Success = rand.Next(0, 1) == 0,
                    Value = rand.Next(100),
                    Message = "message"
                };

                var res = _publish(sd);
                if (res)
                    published++;
                else
                    dropped++;
            }

            Interlocked.Add(ref _eventsPublished, published);
            Interlocked.Add(ref _eventsDropped, dropped);   
        }

        private void ReportData(object state)
        {
            var published = Interlocked.Exchange(ref _eventsPublished, 0);
            var dropped = Interlocked.Exchange(ref _eventsDropped, 0);
       
            _logger.LogInformation("Generated data, {published} events published, {dropped} events dropped",
                published, dropped);
        }
       

        public void Dispose()
        {
            _reportTimer.Dispose();
            _eventTimer.Dispose();
        }
    }
}
