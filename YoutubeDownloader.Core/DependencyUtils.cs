using YoutubeDownloader.Core.Infrastructure;

namespace YoutubeDownloader.Core;

public static class DependencyUtils
{
    private static readonly SystemExecutableLocator Locator = new();

    public static string GetExecutablePath(string toolName, Action<string>? log = null)
    {
        return Locator.Locate(toolName, log);
    }
}
