using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Infrastructure;
using YoutubeDownloader.Core.Services;

namespace YoutubeDownloader.Core.Composition;

public static class CoreServiceFactory
{
    public static IDependencyService CreateDependencyService(
        IExecutableLocator? executableLocator = null,
        IProcessRunner? processRunner = null)
    {
        var locator = executableLocator ?? new SystemExecutableLocator();
        var runner = processRunner ?? new SystemProcessRunner();
        return new DependencyService(locator, runner);
    }

    public static IDownloadClient CreateDownloadClient(
        IExecutableLocator? executableLocator = null,
        IProcessRunner? processRunner = null,
        IProgressParser? progressParser = null)
    {
        var locator = executableLocator ?? new SystemExecutableLocator();
        var runner = processRunner ?? new SystemProcessRunner();
        var parser = progressParser ?? new DefaultProgressParser();

        return new DownloadClient(locator, runner, parser);
    }
}
