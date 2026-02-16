namespace YoutubeDownloader.Core.Abstractions;

public interface IExecutableLocator
{
    string Locate(string toolName, Action<string>? debugLog = null);
}
