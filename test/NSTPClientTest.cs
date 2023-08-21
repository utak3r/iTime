using Xunit;

namespace iTime.Tests;

public class NSTPClientTest
{
    private DateTime RoundTime(DateTime time, TimeSpan span)
    {
        long ticks = (time.Ticks + (span.Ticks / 2) + 1)/ span.Ticks;
        return new DateTime(ticks * span.Ticks, time.Kind);
    }

    [Fact]
    public void TimestampsTest()
    {
        SNTPClient client = new SNTPClient();
        DateTime now = RoundTime(DateTime.Now, TimeSpan.FromSeconds(1));
        DateTime timestamp = RoundTime(client.DestinationTimestamp, TimeSpan.FromSeconds(1));
        Assert.True(timestamp == now);
    }
}
