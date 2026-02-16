using System.Runtime.InteropServices;
using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Services;

public sealed class DownloadClient(
    IExecutableLocator executableLocator,
    IProcessRunner processRunner,
    IProgressParser progressParser) : IDownloadClient
{
    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        Action<string>? onOutput = null,
        Action<string>? onError = null,
        Action<DownloadProgressInfo>? onProgress = null,
        Action<string>? onDebug = null,
        CancellationToken cancellationToken = default)
    {
        var ffmpegPath = executableLocator.Locate("ffmpeg", onDebug);
        var ytDlpPath = executableLocator.Locate("yt-dlp", onDebug);
        var arguments = BuildArguments(request.VideoUrl, request.OutputDirectory, ffmpegPath);

        var environment = BuildEnvironment(ffmpegPath);
        var result = await processRunner.RunAsync(
            new ProcessRunRequest
            {
                FileName = ytDlpPath,
                Arguments = arguments,
                EnvironmentVariables = environment
            },
            onOutput: line =>
            {
                if (progressParser.TryParse(line, out var progress))
                {
                    onProgress?.Invoke(progress);
                    return;
                }

                onOutput?.Invoke(line);
            },
            onError: onError,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode == 0)
        {
            onProgress?.Invoke(new DownloadProgressInfo(100, TimeSpan.Zero, string.Empty));
        }

        return new DownloadResult(result.ExitCode);
    }

    private static string BuildArguments(string videoUrl, string? outputDirectory, string ffmpegPath)
    {
        var outputTemplate = "%(title)s.%(ext)s";
        var baseArguments = $"-f \"bestvideo+bestaudio/best\" --merge-output-format mp4 --ffmpeg-location \"{ffmpegPath}\" -o \"{outputTemplate}\" --newline \"{videoUrl}\"";

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            return baseArguments;
        }

        var normalized = outputDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return $"-P \"{normalized}\" {baseArguments}";
    }

    private static IReadOnlyDictionary<string, string>? BuildEnvironment(string ffmpegPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return null;
        }

        var ffmpegDir = Path.GetDirectoryName(ffmpegPath) ?? string.Empty;
        var existingPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var mergedPath = $"/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin:{existingPath}";

        if (!string.IsNullOrWhiteSpace(ffmpegDir) && !mergedPath.Contains(ffmpegDir, StringComparison.Ordinal))
        {
            mergedPath = $"{ffmpegDir}:{mergedPath}";
        }

        return new Dictionary<string, string>
        {
            ["PATH"] = mergedPath
        };
    }
}
