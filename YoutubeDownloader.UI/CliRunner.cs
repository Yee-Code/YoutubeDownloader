using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Composition;
using YoutubeDownloader.Core.Models;
using YoutubeDownloader.Core.Services;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.UI;

public static class CliRunner
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    private const int ATTACH_PARENT_PROCESS = -1;

    public static async Task RunAsync(string[] args)
    {
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

        var dependencyService = CoreServiceFactory.CreateDependencyService();
        var downloadClient = CoreServiceFactory.CreateDownloadClient();

        var dependencies = await dependencyService.CheckAsync();
        foreach (var dependency in dependencies)
        {
            if (dependency.IsInstalled)
            {
                Console.WriteLine($"{dependency.ExecutableName} detected.");
                continue;
            }

            var severityPrefix = dependency.Type == DependencyType.YtDlp ? "Error" : "Warning";
            Console.WriteLine($"{severityPrefix}: '{dependency.ExecutableName}' is not found.");
            foreach (var hint in DependencyGuidance.GetInstallHints(dependency.Type))
            {
                Console.WriteLine(hint);
            }
        }

        if (!dependencies.Any(x => x.Type == DependencyType.YtDlp && x.IsInstalled))
        {
            return;
        }

        try
        {
            Console.WriteLine($"Starting download: {videoUrl}...");
            var result = await downloadClient.DownloadAsync(
                new DownloadRequest(videoUrl, OutputDirectory: null),
                onOutput: Console.WriteLine,
                onError: line => Console.WriteLine($"[Error/Info] {line}"),
                onProgress: progress =>
                {
                    if (Console.IsOutputRedirected)
                    {
                        return;
                    }

                    try
                    {
                        const int width = 50;
                        var filled = (int)(width * progress.Percentage / 100);
                        var bar = new string('#', filled) + new string('-', width - filled);
                        Console.Write($"\rProgress: [{bar}] {progress.Percentage:F1}%");
                    }
                    catch
                    {
                        // Ignore console errors
                    }
                });

            if (result.IsSuccess)
            {
                Console.WriteLine("\nDownload completed successfully!");
            }
            else
            {
                Console.WriteLine($"\nDownload failed with exit code {result.ExitCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
        }
    }
}
