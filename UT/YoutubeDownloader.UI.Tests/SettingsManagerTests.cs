using System;
using System.IO;
using Xunit;
using YoutubeDownloader.UI.Services;

namespace YoutubeDownloader.UI.Tests;

public sealed class SettingsManagerTests
{
    [Fact]
    public void SaveToPath_DoesNotPersistLastVideoUrl()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "ytdownloader-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string settingsPath = Path.Combine(tempDir, "settings.json");
            var settings = new AppSettings
            {
                LastVideoUrl = "https://www.youtube.com/watch?v=abc123&list=private",
                LastDownloadPath = "/tmp/downloads",
                WindowWidth = 800,
                WindowHeight = 700,
                WindowX = 10,
                WindowY = 20
            };

            SettingsManager.SaveToPath(settingsPath, settings);

            string json = File.ReadAllText(settingsPath);
            Assert.DoesNotContain("abc123", json, StringComparison.Ordinal);
            Assert.DoesNotContain("LastVideoUrl", json, StringComparison.Ordinal);
            Assert.Contains("LastDownloadPath", json, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void LoadFromPath_ClearsLastVideoUrlFromLegacySettings()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "ytdownloader-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string settingsPath = Path.Combine(tempDir, "settings.json");
            File.WriteAllText(settingsPath, """
{
  "LastVideoUrl": "https://www.youtube.com/watch?v=legacy",
  "LastDownloadPath": "/tmp/legacy",
  "WindowWidth": 1200,
  "WindowHeight": 700,
  "WindowX": 30,
  "WindowY": 40
}
""");

            AppSettings loaded = SettingsManager.LoadFromPath(settingsPath);

            Assert.Null(loaded.LastVideoUrl);
            Assert.Equal("/tmp/legacy", loaded.LastDownloadPath);
            Assert.Equal(1200, loaded.WindowWidth);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
