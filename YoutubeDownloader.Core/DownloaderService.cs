using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core
{
    public class DownloaderService
    {
        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;

        public async Task<bool> IsYtDlpInstalledAsync()
        {
            return await CheckToolInstallationAsync("yt-dlp", "--version");
        }

        public async Task<bool> IsFfmpegInstalledAsync()
        {
            return await CheckToolInstallationAsync("ffmpeg", "-version");
        }

        private async Task<bool> CheckToolInstallationAsync(string toolName, string arguments)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = toolName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    await process.WaitForExitAsync();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> DownloadVideoAsync(string videoUrl)
        {
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
                process.OutputDataReceived += (sender, e) => { if (e.Data != null) OutputReceived?.Invoke(this, e.Data); };
                process.ErrorDataReceived += (sender, e) => { if (e.Data != null) ErrorReceived?.Invoke(this, e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                return process.ExitCode;
            }
        }
    }
}
