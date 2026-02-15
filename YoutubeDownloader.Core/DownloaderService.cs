using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core
{
    public class DownloaderService
    {
        private static readonly Regex ProgressRegex = new(@"\[download\]\s+(\d+\.?\d*)%", RegexOptions.Compiled);
        private static readonly Regex EtaRegex = new(@"ETA\s+((?:\d{1,2}:)?\d{1,2}:\d{2})", RegexOptions.Compiled);

        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;
        public event EventHandler<TimeSpan?>? EstimatedTimeRemainingChanged;
        public event EventHandler<string>? DebugLogReceived;

        public async Task<bool> IsYtDlpInstalledAsync()
        {
            var toolName = DependencyUtils.GetExecutablePath("yt-dlp", msg => DebugLogReceived?.Invoke(this, msg));
            return await CheckToolInstallationAsync(toolName, "--version");
        }

        public async Task<bool> IsFfmpegInstalledAsync()
        {
            var toolName = DependencyUtils.GetExecutablePath("ffmpeg", msg => DebugLogReceived?.Invoke(this, msg));
            return await CheckToolInstallationAsync(toolName, "-version");
        }

        public async Task<bool> IsNodeInstalledAsync()
        {
            var toolName = DependencyUtils.GetExecutablePath("node", msg => DebugLogReceived?.Invoke(this, msg));
            return await CheckToolInstallationAsync(toolName, "--version");
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

        public event EventHandler<double>? ProgressChanged;

        public async Task<int> DownloadVideoAsync(string videoUrl, string? outputDirectory = null)
        {
            // Construct arguments for yt-dlp
            // -f "bestvideo+bestaudio/best" : Download best video and best audio and merge them. Fallback to best single file.
            // --merge-output-format mp4 : Ensure output is mp4
            // -o "%(title)s.%(ext)s" : Output filename format
            // --newline: Output progress on new lines for easier parsing
            string outputTemplate = "%(title)s.%(ext)s";
            string arguments = $"-f \"bestvideo+bestaudio/best\" --merge-output-format mp4 -o \"{outputTemplate}\" --newline \"{videoUrl}\"";

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                // Fix: Trim trailing backslash to prevent escaping the closing quote
                outputDirectory = outputDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                arguments = $"-P \"{outputDirectory}\" " + arguments;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = DependencyUtils.GetExecutablePath("yt-dlp"),
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        // Parse progress
                        var match = ProgressRegex.Match(e.Data);
                        if (match.Success && double.TryParse(match.Groups[1].Value, out double progress))
                        {
                            ProgressChanged?.Invoke(this, progress);

                            TimeSpan? eta = TryParseEta(e.Data);
                            EstimatedTimeRemainingChanged?.Invoke(this, eta);
                        }
                        else
                        {
                            OutputReceived?.Invoke(this, e.Data);
                        }
                    }
                };
                process.ErrorDataReceived += (sender, e) => { if (e.Data != null) ErrorReceived?.Invoke(this, e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
                EstimatedTimeRemainingChanged?.Invoke(this, null);

                return process.ExitCode;
            }
        }

        private static TimeSpan? TryParseEta(string outputLine)
        {
            var match = EtaRegex.Match(outputLine);
            if (!match.Success)
            {
                return null;
            }

            if (TimeSpan.TryParse(match.Groups[1].Value, out var eta))
            {
                return eta;
            }

            return null;
        }
    }
}
