using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Abstractions;

public interface IProcessRunner
{
    Task<ProcessRunResult> RunAsync(
        ProcessRunRequest request,
        Action<string>? onOutput = null,
        Action<string>? onError = null,
        CancellationToken cancellationToken = default);
}
