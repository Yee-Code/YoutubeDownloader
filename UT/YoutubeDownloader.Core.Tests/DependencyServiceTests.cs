using Xunit;
using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Models;
using YoutubeDownloader.Core.Services;

namespace YoutubeDownloader.Core.Tests;

public sealed class DependencyServiceTests
{
    [Fact]
    public async Task CheckAsync_UsesLocatorAndRunnerResults()
    {
        var locator = new FakeLocator();
        var runner = new FakeRunner
        {
            ExitCodes =
            {
                ["yt-dlp"] = 0,
                ["ffmpeg"] = 1,
                ["node"] = 0
            }
        };

        var service = new DependencyService(locator, runner);

        var result = await service.CheckAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result.Single(x => x.Type == DependencyType.YtDlp).IsInstalled);
        Assert.False(result.Single(x => x.Type == DependencyType.Ffmpeg).IsInstalled);
        Assert.True(result.Single(x => x.Type == DependencyType.Node).IsInstalled);
    }

    private sealed class FakeLocator : IExecutableLocator
    {
        public string Locate(string toolName, Action<string>? debugLog = null)
        {
            return toolName;
        }
    }

    private sealed class FakeRunner : IProcessRunner
    {
        public Dictionary<string, int> ExitCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<ProcessRunResult> RunAsync(
            ProcessRunRequest request,
            Action<string>? onOutput = null,
            Action<string>? onError = null,
            CancellationToken cancellationToken = default)
        {
            var code = ExitCodes.TryGetValue(request.FileName, out var value) ? value : 1;
            return Task.FromResult(new ProcessRunResult(code));
        }
    }
}
