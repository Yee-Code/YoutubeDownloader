namespace YoutubeDownloader.Core.Models;

public sealed record DownloadResult(int ExitCode)
{
    public bool IsSuccess => ExitCode == 0;
}
