using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.DataPipeline
{
    public class AzureBlobPublisherPipeline<TInput> : BatchingWindowBase<TInput, byte[]>
    {
        // TODO - add metrics to this class

        private readonly Func<string> _blobNameFunc;
        private readonly CloudBlobContainer _container;

        public AzureBlobPublisherPipeline(BatchingWindowConfiguration config,
            CloudBlobContainer container, 
            Func<string> blobNameFunc,
            ILogger logger)
            : base(config, logger) 
        {
            this._blobNameFunc = blobNameFunc;
            this._container = container;
        }


        protected override async Task Publish(IEnumerable<byte[]> evts)
        {
            foreach (var e in evts)
            {
                string blobName = "Unknown";
                try
                {
                    blobName = _blobNameFunc();
                    var blobRef = _container.GetBlockBlobReference(blobName);

                    await blobRef.UploadFromByteArrayAsync(e, 0, e.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, "Could not publish blob {blobName} to {containerName} on account {account}",
                        blobName, _container.Name, _container.StorageUri.ToString());                  
                }
            }
        }

        protected override IEnumerable<byte[]> Transform(IEnumerable<TInput> evts)
        {            
            try
            {
                // TODO; handle compresion
                using (var ms = new MemoryStream())
                {
                    foreach (var e in evts)
                    {
                        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                        ms.Write(data, 0, data.Length);
                        ms.WriteByte((byte)'\n');                        
                    }

                    return new byte[][] { ms.ToArray() };
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Could not transform events");               
                return Enumerable.Empty<byte[]>();
            }           
        }
    }
}
