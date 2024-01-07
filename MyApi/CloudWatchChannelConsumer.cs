using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using System.Threading.Channels;

public class CloudWatchChannelConsumer : BackgroundService
{
    private readonly Channel<string> _channel;

    private readonly IAmazonCloudWatch _amazonCloudWatch;

    private readonly string _host;

    private readonly ILogger<CloudWatchChannelConsumer> _logger;

    public CloudWatchChannelConsumer(Channel<string> channel, IAmazonCloudWatch amazonCloudWatch, ILogger<CloudWatchChannelConsumer> logger)
    {
        _channel = channel;
        _host = Environment.GetEnvironmentVariable("POD_NAME") ?? "local";
        _logger = logger;
        _amazonCloudWatch = amazonCloudWatch;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!_channel.Reader.Completion.IsCompleted && await _channel.Reader.WaitToReadAsync())
        {
            if (_channel.Reader.TryRead(out var value))
            {
                try
                {
                    await _amazonCloudWatch.PutMetricDataAsync(new PutMetricDataRequest
                    {
                        Namespace = "MyApiNamespace",
                        MetricData =
                    [
                        new MetricDatum
                        {
                            MetricName = "RequestPerSecond",
                            Value = double.Parse(value),
                            Unit = StandardUnit.CountSecond,
                            TimestampUtc = DateTime.UtcNow,
                            Dimensions = new List<Dimension>
                            {
                                new() {
                                    Name = "Host",
                                    Value = _host
                                }
                            }
                        }
                    ]
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send CloudWatch Metric");
                }
            }
        }
    }
}