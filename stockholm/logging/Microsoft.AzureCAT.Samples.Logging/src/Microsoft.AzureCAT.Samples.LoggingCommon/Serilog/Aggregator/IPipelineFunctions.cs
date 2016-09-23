using Serilog.Events;

namespace Microsoft.AzureCAT.Samples.Logging
{
    internal interface IPipelineFunctions
    {
        bool Filter(LogEvent evt);
        string GetName(LogEvent evt);
        string GetUnit(LogEvent evt);
        decimal GetValue(LogEvent evt);
    }
}