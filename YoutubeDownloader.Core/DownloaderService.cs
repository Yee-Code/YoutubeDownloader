using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Composition;
using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core;

public class DownloaderService
{
    private readonly IDependencyService _dependencyService;
    private readonly IDownloadClient _downloadClient;

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<string>? ErrorReceived;
    public event EventHandler<TimeSpan?>? EstimatedTimeRemainingChanged;
    public event EventHandler<string>? DebugLogReceived;
    public event EventHandler<double>? ProgressChanged;

    public DownloaderService()
        : this(CoreServiceFactory.CreateDependencyService(), CoreServiceFactory.CreateDownloadClient())
    {
    }

    public DownloaderService(IDependencyService dependencyService, IDownloadClient downloadClient)
    {
        _dependencyService = dependencyService;
        _downloadClient = downloadClient;
    }

    public async Task<bool> IsYtDlpInstalledAsync()
    {
        var results = await _dependencyService.CheckAsync().ConfigureAwait(false);
        return results.FirstOrDefault(x => x.Type == DependencyType.YtDlp)?.IsInstalled == true;
    }

    public async Task<bool> IsFfmpegInstalledAsync()
    {
        var results = await _dependencyService.CheckAsync().ConfigureAwait(false);
        return results.FirstOrDefault(x => x.Type == DependencyType.Ffmpeg)?.IsInstalled == true;
    }

    public async Task<bool> IsNodeInstalledAsync()
    {
        var results = await _dependencyService.CheckAsync().ConfigureAwait(false);
        return results.FirstOrDefault(x => x.Type == DependencyType.Node)?.IsInstalled == true;
    }

    public async Task<int> DownloadVideoAsync(string videoUrl, string? outputDirectory = null)
    {
        var result = await _downloadClient.DownloadAsync(
            new DownloadRequest(videoUrl, outputDirectory),
            onOutput: line => OutputReceived?.Invoke(this, line),
            onError: line => ErrorReceived?.Invoke(this, line),
            onProgress: progress =>
            {
                ProgressChanged?.Invoke(this, progress.Percentage);
                EstimatedTimeRemainingChanged?.Invoke(this, progress.EstimatedTimeRemaining);
            },
            onDebug: message => DebugLogReceived?.Invoke(this, message)).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            EstimatedTimeRemainingChanged?.Invoke(this, null);
        }

        return result.ExitCode;
    }
}
