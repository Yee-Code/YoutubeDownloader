using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Abstractions;

public interface IDependencyService
{
    Task<IReadOnlyList<DependencyCheckResult>> CheckAsync(CancellationToken cancellationToken = default);
}
