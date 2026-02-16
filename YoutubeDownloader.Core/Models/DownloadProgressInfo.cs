namespace YoutubeDownloader.Core.Models;

public sealed record DownloadProgressInfo(double Percentage, TimeSpan? EstimatedTimeRemaining, string RawLine);
