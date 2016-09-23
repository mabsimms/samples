namespace Kyobi.Logging.Sinks.StatsAggregator
{
    public interface IPublisherConfig
    {
        string Server { get; set; }
        int Port { get; set; }
    }
}