using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace YoutubeDownloader.Core
{
    public static class DependencyUtils
    {
        public static string GetExecutablePath(string toolName, Action<string>? log = null)
        {
            // Add extension for Windows if not present
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !toolName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                toolName += ".exe";
            }

            log?.Invoke($"[DependencyUtils] Looking for: {toolName}");

            // 1. Check local directory (AppDomain BaseDirectory)
            var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, toolName);
            log?.Invoke($"[DependencyUtils] Checking local directory: {localPath}");
            if (File.Exists(localPath))
            {
                log?.Invoke($"[DependencyUtils] Found in local directory: {localPath}");
                return localPath;
            }

            // 2. Check PATH environment variable
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                log?.Invoke($"[DependencyUtils] Checking PATH ({paths.Length} entries)...");
                foreach (var path in paths)
                {
                    try
                    {
                        var fullPath = Path.Combine(path, toolName);
                        if (File.Exists(fullPath))
                        {
                            log?.Invoke($"[DependencyUtils] Found in PATH: {fullPath}");
                            return fullPath;
                        }
                    }
                    catch
                    {
                        // Ignore invalid paths in PATH
                    }
                }
                log?.Invoke($"[DependencyUtils] Not found in PATH");
            }
            else
            {
                log?.Invoke($"[DependencyUtils] PATH environment variable is empty");
            }

            // 3. (macOS/Linux) Check common install locations for Homebrew/MacPorts
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string[] commonPaths =
                [
                    "/opt/homebrew/bin", // Apple Silicon Homebrew
                    "/usr/local/bin",    // Intel Homebrew
                    "/usr/bin",
                    "/bin"
                ];

                log?.Invoke($"[DependencyUtils] Checking common install locations...");
                foreach (var path in commonPaths)
                {
                    var fullPath = Path.Combine(path, toolName);
                    log?.Invoke($"[DependencyUtils] Checking: {fullPath}");
                    if (File.Exists(fullPath))
                    {
                        log?.Invoke($"[DependencyUtils] Found in common location: {fullPath}");
                        return fullPath;
                    }
                }
            }

            // Return original tool name to let Process.Start try its default resolution (or fail)
            log?.Invoke($"[DependencyUtils] Not found anywhere, returning: {toolName}");
            return toolName;
        }
    }
}
