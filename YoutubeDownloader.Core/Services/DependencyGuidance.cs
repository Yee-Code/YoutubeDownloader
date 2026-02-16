using System.Runtime.InteropServices;
using YoutubeDownloader.Core.Models;

namespace YoutubeDownloader.Core.Services;

public static class DependencyGuidance
{
    public static IEnumerable<string> GetInstallHints(DependencyType type)
    {
        return type switch
        {
            DependencyType.YtDlp => GetPerPlatformHints(
                "Please install it via Winget: winget install yt-dlp",
                "Please install it via Homebrew: brew install yt-dlp"),
            DependencyType.Ffmpeg => GetPerPlatformHints(
                "Please install it via Winget: winget install Gyan.FFmpeg",
                "Please install it via Homebrew: brew install ffmpeg"),
            DependencyType.Node => GetPerPlatformHints(
                "Please install it via Winget: winget install OpenJS.NodeJS",
                "Please install it via Homebrew: brew install node"),
            _ => ["Please install it via your package manager."]
        };
    }

    private static IEnumerable<string> GetPerPlatformHints(string windowsHint, string macHint)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return [windowsHint];
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return [macHint];
        }

        return ["Please install it via your package manager."];
    }
}
