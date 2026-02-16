using System.Globalization;
using System.Text.RegularExpressions;
using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Services;

public sealed class DefaultProgressParser : IProgressParser
{
    private static readonly Regex ProgressRegex = new(@"\[download\]\s+(\d+\.?\d*)%", RegexOptions.Compiled);
    private static readonly Regex EtaRegex = new(@"ETA\s+((?:\d{1,2}:)?\d{1,2}:\d{2})", RegexOptions.Compiled);

    public bool TryParse(string outputLine, out DownloadProgressInfo progressInfo)
    {
        var match = ProgressRegex.Match(outputLine);
        if (!match.Success ||
            !double.TryParse(match.Groups[1].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var progress))
        {
            progressInfo = default!;
            return false;
        }

        progressInfo = new DownloadProgressInfo(progress, TryParseEta(outputLine), outputLine);
        return true;
    }

    private static TimeSpan? TryParseEta(string outputLine)
    {
        var etaMatch = EtaRegex.Match(outputLine);
        if (!etaMatch.Success)
        {
            return null;
        }

        return TimeSpan.TryParse(etaMatch.Groups[1].Value, out var eta) ? eta : null;
    }
}
