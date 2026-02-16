using Xunit;
using YoutubeDownloader.Core.Services;

namespace YoutubeDownloader.Core.Tests;

public sealed class DefaultProgressParserTests
{
    [Fact]
    public void TryParse_ReturnsTrue_WhenProgressLineContainsPercentageAndEta()
    {
        var parser = new DefaultProgressParser();

        var parsed = parser.TryParse("[download]  42.5% of 10.00MiB at 1.00MiB/s ETA 00:11", out var progress);

        Assert.True(parsed);
        Assert.Equal(42.5, progress.Percentage, 3);
        Assert.Equal(TimeSpan.FromMinutes(11), progress.EstimatedTimeRemaining);
    }

    [Fact]
    public void TryParse_ReturnsFalse_ForNonProgressLine()
    {
        var parser = new DefaultProgressParser();

        var parsed = parser.TryParse("[info] Extracting URL", out _);

        Assert.False(parsed);
    }
}
