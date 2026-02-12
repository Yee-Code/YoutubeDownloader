using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Downloader (via yt-dlp)");
            Console.WriteLine("-------------------------------");

            // Check if yt-dlp is installed
            if (!IsYtDlpInstalled())
            {
                Console.WriteLine("Error: 'yt-dlp' is not found in PATH.");
                Console.WriteLine("Please install it via Homebrew: brew install yt-dlp");
                return;
            }

            // Check if ffmpeg is installed (recommended for merging)
            if (!IsFfmpegInstalled())
            {
                Console.WriteLine("Warning: 'ffmpeg' is not found. High quality video/audio merging might fail.");
                Console.WriteLine("Install it via: brew install ffmpeg");
            }

            while (true)
            {
                Console.Write("\nEnter YouTube URL (or 'exit' to quit): ");
                var videoUrl = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(videoUrl)) continue;
                if (videoUrl.ToLower() == "exit") break;

                try
                {
                    Console.WriteLine("Starting download with yt-dlp...");

                    // Construct arguments for yt-dlp
                    // -f "bestvideo+bestaudio/best" : Download best video and best audio and merge them. Fallback to best single file.
                    // --merge-output-format mp4 : Ensure output is mp4
                    // -o "%(title)s.%(ext)s" : Output filename format
                    string arguments = $"-f \"bestvideo+bestaudio/best\" --merge-output-format mp4 -o \"%(title)s.%(ext)s\" \"{videoUrl}\"";

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process())
                    {
                        process.StartInfo = processStartInfo;
                        process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine($"[Error/Info] {e.Data}"); };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine("\nDownload completed successfully!");
                        }
                        else
                        {
                            Console.WriteLine($"\nDownload failed with exit code {process.ExitCode}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
                }
            }
        }

        static bool IsYtDlpInstalled()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "yt-dlp";
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        static bool IsFfmpegInstalled()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "ffmpeg";
                    process.StartInfo.Arguments = "-version";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
