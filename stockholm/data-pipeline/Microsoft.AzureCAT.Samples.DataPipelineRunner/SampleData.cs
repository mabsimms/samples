using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.DataPipelineRunner
{
    public class SampleData
    {
        public DateTime Timestamp { get; set; }
        public string SourceHost { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public double Value { get; set; }
    }
}
