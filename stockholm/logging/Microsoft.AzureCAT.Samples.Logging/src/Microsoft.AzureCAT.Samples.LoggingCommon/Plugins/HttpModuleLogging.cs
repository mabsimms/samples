namespace Microsoft.AzureCAT.Samples.Logging.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AzureCAT.Samples.Logging.Extensions;
    using Microsoft.Extensions.Logging;

    public class HttpLoggingModule
    {
        private readonly ILogger<HttpLoggingModule> _logger;
        private readonly RequestDelegate _next;

        public HttpLoggingModule(RequestDelegate next, 
            ILogger<HttpLoggingModule> logger)
        {
            this._logger = logger;
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var startTime = Stopwatch.GetTimestamp();
            try
            {
                await _next(context);
            }
            finally
            {
                var timestamp = Stopwatch.GetTimestamp();
                var requestProcessingTimeInMs = (timestamp - startTime)
                    * 1000 / Stopwatch.Frequency;

              
                // Get the request name
                // TODO - get a logger from ioc
                _logger.LogTiming(context.Request.Path,
                    TimeSpan.FromMilliseconds(requestProcessingTimeInMs));
            }
        }
     }
}
