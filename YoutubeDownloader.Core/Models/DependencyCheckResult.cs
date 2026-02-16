namespace YoutubeDownloader.Core.Models;

public sealed record DependencyCheckResult(
    DependencyType Type,
    string ExecutableName,
    string ResolvedPath,
    bool IsInstalled);
