using Microsoft.AzureCAT.Samples.DataPipelineRunner;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.DataPipeline
{
    public class DemoClass<T>
    {
        private readonly ILogger logger;

        public DemoClass(ILogger logger)
        {
            this.logger = logger;
        }

        public void DoSomethingCool(SampleData data)
        {

        }
    }
}
