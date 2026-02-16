using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Services;

public sealed class DependencyService(IExecutableLocator executableLocator, IProcessRunner processRunner) : IDependencyService
{
    public async Task<IReadOnlyList<DependencyCheckResult>> CheckAsync(CancellationToken cancellationToken = default)
    {
        var checks = new[]
        {
            CheckAsync(DependencyType.YtDlp, "yt-dlp", "--version", cancellationToken),
            CheckAsync(DependencyType.Ffmpeg, "ffmpeg", "-version", cancellationToken),
            CheckAsync(DependencyType.Node, "node", "--version", cancellationToken)
        };

        return await Task.WhenAll(checks).ConfigureAwait(false);
    }

    private async Task<DependencyCheckResult> CheckAsync(
        DependencyType type,
        string executableName,
        string arguments,
        CancellationToken cancellationToken)
    {
        var resolvedPath = executableLocator.Locate(executableName);

        try
        {
            var result = await processRunner.RunAsync(
                new Models.ProcessRunRequest
                {
                    FileName = resolvedPath,
                    Arguments = arguments
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return new DependencyCheckResult(type, executableName, resolvedPath, result.ExitCode == 0);
        }
        catch
        {
            return new DependencyCheckResult(type, executableName, resolvedPath, false);
        }
    }
}
