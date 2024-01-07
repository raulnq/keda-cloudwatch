using System.Diagnostics.Tracing;
using System.Threading.Channels;

public class RequestPerSecondChannelProducer : EventListener
{
    private readonly Channel<string> _channel;

    public RequestPerSecondChannelProducer(Channel<string> channel)
    {
        _channel = channel;
    }

    protected override void OnEventSourceCreated(EventSource source)
    {
        if (source.Name.Equals("Microsoft.AspNetCore.Hosting"))
        {
            EnableEvents(source, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string?>()
            {
                ["EventCounterIntervalSec"] = "1"
            });
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData?.EventName != null && eventData.EventName.Equals("EventCounters", StringComparison.InvariantCulture) && eventData.Payload != null)
        {
            for (int i = 0; i < eventData.Payload.Count; i++)
            {
                var eventPayload = eventData.Payload[i] as IDictionary<string, object>;

                if (eventPayload != null && eventPayload.Any(item => item.Value != null && item.Value.ToString()!.Equals("requests-per-second", StringComparison.InvariantCulture)))
                {
                    var value = eventPayload.First(item => item.Key != null && item.Key.Equals("Increment", StringComparison.InvariantCulture));

                    if (value.Value != null)
                    {
                        _channel.Writer.TryWrite(value.Value.ToString()!);
                    }
                }
            }
        }
    }
}