using System;
using System.Threading.Tasks;
using YoutubeDownloader.Core;

namespace YoutubeDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("YouTube Downloader (via yt-dlp)");
            Console.WriteLine("-------------------------------");

            var downloader = new DownloaderService();

            // Check if yt-dlp is installed
            if (!await downloader.IsYtDlpInstalledAsync())
            {
                Console.WriteLine("Error: 'yt-dlp' is not found in PATH.");
                Console.WriteLine("Please install it via Homebrew: brew install yt-dlp");
                return;
            }

            // Check if ffmpeg is installed (recommended for merging)
            if (!await downloader.IsFfmpegInstalledAsync())
            {
                Console.WriteLine("Warning: 'ffmpeg' is not found. High quality video/audio merging might fail.");
                Console.WriteLine("Install it via: brew install ffmpeg");
            }

            // Subscribe to events
            downloader.OutputReceived += (sender, data) => Console.WriteLine(data);
            downloader.ErrorReceived += (sender, data) => Console.WriteLine($"[Error/Info] {data}");

            while (true)
            {
                Console.Write("\nEnter YouTube URL (or 'exit' to quit): ");
                var videoUrl = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(videoUrl)) continue;
                if (videoUrl.ToLower() == "exit") break;

                try
                {
                    Console.WriteLine("Starting download with yt-dlp...");
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
    }
}
