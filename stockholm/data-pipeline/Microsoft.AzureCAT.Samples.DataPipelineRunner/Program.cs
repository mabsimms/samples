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
                CompressAfterTransform = false,
                MaxBacklogSize = 1024,
                MaxPublishBacklogSize = 4,
                MaxWindowEventCount = 32000,
                PublishDegreeOfParallelism = 2,
                SlidingWindowSize = TimeSpan.FromSeconds(30)
            };

            var blobPublisher = new AzureBlobPublisherPipeline<SampleData>(
                config: batchConfiguration,
                container: container,
                blobNameFunc: GetBlobName,
                logger: logger);

            // Create the sample data generator (TODO - base this on config)
            var dataGenerator = new SampleDataGenerator(
                period: 1000,
                maxEventsPerTick: 1,
                pub: blobPublisher.Enqueue
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
