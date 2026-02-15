using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace YoutubeDownloader.Core
{
    public static class DependencyUtils
    {
        public static string GetExecutablePath(string toolName)
        {
            // Add extension for Windows if not present
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !toolName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                toolName += ".exe";
            }

            // 1. Check local directory (AppDomain BaseDirectory)
            var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, toolName);
            if (File.Exists(localPath))
            {
                return localPath;
            }

            // 2. Check PATH environment variable
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    try
                    {
                        var fullPath = Path.Combine(path, toolName);
                        if (File.Exists(fullPath))
                        {
                            return fullPath;
                        }
                    }
                    catch
                    {
                        // Ignore invalid paths in PATH
                    }
                }
            }

            // Return original tool name to let Process.Start try its default resolution (or fail)
            return toolName;
        }
    }
}
