using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;

namespace YoutubeDownloader.UI.Services
{
    public class AppSettings
    {
        public string? LastVideoUrl { get; set; }
        public string? LastDownloadPath { get; set; }
        public double WindowWidth { get; set; } = 800;
        public double WindowHeight { get; set; } = 450;
        public int WindowX { get; set; } = -1;
        public int WindowY { get; set; } = -1;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeDownloader",
            "settings.json");

        public static AppSettings Load()
        {
            return LoadFromPath(SettingsFilePath);
        }

        internal static AppSettings LoadFromPath(string settingsFilePath)
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                    // Privacy-by-default: never keep previously saved URL in memory.
                    settings.LastVideoUrl = null;
                    return settings;
                }
            }
            catch
            {
                // Ignore errors and return default
            }
            return new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            SaveToPath(SettingsFilePath, settings);
        }

        internal static void SaveToPath(string settingsFilePath, AppSettings settings)
        {
            try
            {
                string? directory = Path.GetDirectoryName(settingsFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Do not persist user-entered URL to disk.
                var sanitizedSettings = new AppSettings
                {
                    LastVideoUrl = null,
                    LastDownloadPath = settings.LastDownloadPath,
                    WindowWidth = settings.WindowWidth,
                    WindowHeight = settings.WindowHeight,
                    WindowX = settings.WindowX,
                    WindowY = settings.WindowY
                };

                string json = JsonSerializer.Serialize(
                    sanitizedSettings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });
                File.WriteAllText(settingsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
