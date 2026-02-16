using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Composition;
using YoutubeDownloader.UI.ViewModels;

namespace YoutubeDownloader.UI.Services;

public sealed class AppBootstrapper
{
    public IDependencyService DependencyService { get; }
    public IDownloadClient DownloadClient { get; }

    public AppBootstrapper()
    {
        DependencyService = CoreServiceFactory.CreateDependencyService();
        DownloadClient = CoreServiceFactory.CreateDownloadClient();
    }

    public MainWindowViewModel CreateMainWindowViewModel()
    {
        return new MainWindowViewModel(DependencyService, DownloadClient);
    }
}
