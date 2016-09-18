using Microsoft.AzureCAT.Samples.DataPipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.DataPipelineRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the configuration context
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")        
                .Build();

            // Create the system logger (TODO - make this real)
            var loggerFactory = new LoggerFactory()
                .AddConsole();
            var logger = loggerFactory.CreateLogger("default");
               
            // Get the storage connection information
            var storageConnectionString = config["storage:connection"];
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the container if necessary
            var containerName = config["storage:container"];
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            if (container.CreateIfNotExistsAsync().Result)
            {
                // container was created
            }
   
            // Create the data pipeline
            var batchConfiguration = new BatchingWindowConfiguration()
            {
                CompressAfterTransform = config.GetValue<bool>("pipeline:compress"),
                MaxBacklogSize = config.GetValue<int>("pipeline:backlog"),
                MaxPublishBacklogSize = config.GetValue<int>("pipeline:publishBacklog"),
                MaxWindowEventCount = config.GetValue<int>("pipeline:maxBatchSize"),
                PublishDegreeOfParallelism = config.GetValue<int>("pipeline:publishConcurrency"),
                SlidingWindowSize = config.GetValue<TimeSpan>("pipeline:maxWindowSize"),
            };

            var blobPublisher = new AzureBlobPublisherPipeline<SampleData>(
                config: batchConfiguration,
                container: container,
                blobNameFunc: GetBlobName,
                logger: logger);

            // Create the sample data generator (TODO - base this on config)
            var dataGenerator = new SampleDataGenerator(
                period: (int)config.GetValue<TimeSpan>("dataGenerator:period").TotalMilliseconds,
                maxEventsPerTick: config.GetValue<int>("dataGenerator:maxEventsPerTick"),
                pub: blobPublisher.Enqueue,
                logger: logger
            );


            // Wait for user command to shut down
            Console.ReadLine();

            dataGenerator.Dispose();
            blobPublisher.Dispose();
          
        }

        public static string GetBlobName()
        {
            return String.Concat(
                DateTime.UtcNow.ToString("yyyyMMddhhmmss"),
                "-",
                Guid.NewGuid().ToString(),
                ".json");
        }
    }
}
