namespace YoutubeDownloader.Core.Models;

public sealed record DownloadRequest(string VideoUrl, string? OutputDirectory);
