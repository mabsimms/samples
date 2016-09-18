namespace Microsoft.AzureCAT.Samples.DataPipeline
{
    public class BatchingWindowConfiguration
    {     
        public int MaxBacklogSize { get; set; }
        public int MaxWindowEventCount { get; set; }
        public int PublishDegreeOfParallelism { get; set; }
        public int MaxPublishBacklogSize { get; set; } = 32;
        public bool CompressAfterTransform { get; set; } = false;
        public System.TimeSpan SlidingWindowSize { get; set; }      
    }
}