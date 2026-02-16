using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Abstractions;

public interface IDownloadClient
{
    Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        Action<string>? onOutput = null,
        Action<string>? onError = null,
        Action<DownloadProgressInfo>? onProgress = null,
        Action<string>? onDebug = null,
        CancellationToken cancellationToken = default);
}
