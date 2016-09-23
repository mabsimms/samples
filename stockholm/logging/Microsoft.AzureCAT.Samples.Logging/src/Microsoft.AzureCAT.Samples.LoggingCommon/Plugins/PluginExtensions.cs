using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AzureCAT.Samples.Logging.Plugins
{
    public static class PluginExtensions
    {
        public static IApplicationBuilder UseHttpLoggingModule(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HttpLoggingModule>();
        }
    }
}
