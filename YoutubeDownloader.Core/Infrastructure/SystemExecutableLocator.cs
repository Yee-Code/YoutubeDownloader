using System.Runtime.InteropServices;
using YoutubeDownloader.Core.Abstractions;

namespace YoutubeDownloader.Core.Infrastructure;

public sealed class SystemExecutableLocator : IExecutableLocator
{
    public string Locate(string toolName, Action<string>? debugLog = null)
    {
        var normalizedName = NormalizeExecutableName(toolName);
        debugLog?.Invoke($"[ExecutableLocator] Looking for: {normalizedName}");

        var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, normalizedName);
        if (File.Exists(localPath))
        {
            debugLog?.Invoke($"[ExecutableLocator] Found in app directory: {localPath}");
            return localPath;
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathEnv))
        {
            foreach (var path in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var candidate = Path.Combine(path, normalizedName);
                    if (File.Exists(candidate))
                    {
                        debugLog?.Invoke($"[ExecutableLocator] Found in PATH: {candidate}");
                        return candidate;
                    }
                }
                catch
                {
                    // Ignore invalid PATH entries
                }
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] commonPaths = ["/opt/homebrew/bin", "/usr/local/bin", "/usr/bin", "/bin"];
            foreach (var basePath in commonPaths)
            {
                var candidate = Path.Combine(basePath, normalizedName);
                if (File.Exists(candidate))
                {
                    debugLog?.Invoke($"[ExecutableLocator] Found in common path: {candidate}");
                    return candidate;
                }
            }
        }

        debugLog?.Invoke($"[ExecutableLocator] Not found, fallback to executable name: {normalizedName}");
        return normalizedName;
    }

    private static string NormalizeExecutableName(string toolName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            !toolName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return toolName + ".exe";
        }

        return toolName;
    }
}
