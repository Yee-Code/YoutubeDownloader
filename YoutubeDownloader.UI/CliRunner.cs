using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YoutubeDownloader.Core;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.UI;

public static class CliRunner
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    private const int ATTACH_PARENT_PROCESS = -1;

    public static async Task RunAsync(string[] args)
    {
        // On Windows, a GUI app doesn't have a console by default.
        // We attempt to attach to the parent process's console.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }

        Console.WriteLine();
        Console.WriteLine($"YouTube Downloader (CLI Mode) v{AppInfo.GetVersion()}");
        Console.WriteLine("-----------------------------");

        var videoUrl = args.Length > 0 ? args[0] : null;

        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            Console.WriteLine("Error: No URL provided.");
            Console.WriteLine("Usage: YoutubeDownloader.exe <url>");
            return;
        }

        var downloader = new DownloaderService();

        // Check if yt-dlp is installed
        if (!await downloader.IsYtDlpInstalledAsync())
        {
            Console.WriteLine("Error: 'yt-dlp' is not found in PATH.");
            Console.WriteLine("Please install it.");
            return;
        }

        // Check if ffmpeg is installed
        if (!await downloader.IsFfmpegInstalledAsync())
        {
            Console.WriteLine("Warning: 'ffmpeg' is not found. High quality video/audio merging might fail.");
        }

        // Check if Node.js is installed
        if (!await downloader.IsNodeInstalledAsync())
        {
            Console.WriteLine("Warning: 'Node.js' is not found. Some videos might fail to download.");
            Console.WriteLine("Please install Node.js from https://nodejs.org/");
        }

        // Subscribe to events
        downloader.OutputReceived += (sender, data) => Console.WriteLine(data);
        downloader.ErrorReceived += (sender, data) => Console.WriteLine($"[Error/Info] {data}");
        downloader.ProgressChanged += (sender, progress) =>
        {
            if (Console.IsOutputRedirected) return;
            try
            {
                int width = 50;
                int filled = (int)(width * progress / 100);
                string bar = new string('#', filled) + new string('-', width - filled);
                Console.Write($"\rProgress: [{bar}] {progress:F1}%");
            }
            catch { /* Ignore console errors */ }
        };

        try
        {
            Console.WriteLine($"Starting download: {videoUrl}...");
            int exitCode = await downloader.DownloadVideoAsync(videoUrl);

            if (exitCode == 0)
            {
                Console.WriteLine("\nDownload completed successfully!");
            }
            else
            {
                Console.WriteLine($"\nDownload failed with exit code {exitCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
        }
    }
}
