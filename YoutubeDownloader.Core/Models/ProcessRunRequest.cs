namespace YoutubeDownloader.Core.Models;

public sealed class ProcessRunRequest
{
    public required string FileName { get; init; }
    public string Arguments { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string>? EnvironmentVariables { get; init; }
}
