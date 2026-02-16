using System;
using System.IO;
using System.Reflection;

namespace YoutubeDownloader.Core.Utils;

public static class AppInfo
{
    public static string GetVersion()
    {
        try
        {
            var versionFile = Path.Combine(AppContext.BaseDirectory, "VERSION");
            if (File.Exists(versionFile))
            {
                return File.ReadAllText(versionFile).Trim();
            }
        }
        catch { }

        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}
