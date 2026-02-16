using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Abstractions;

public interface IProgressParser
{
    bool TryParse(string outputLine, out DownloadProgressInfo progressInfo);
}
