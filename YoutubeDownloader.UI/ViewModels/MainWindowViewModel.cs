using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core;
using Avalonia.Controls;
using System.Linq;
using Avalonia.Platform.Storage;
using System.Runtime.InteropServices;

namespace YoutubeDownloader.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DownloaderService _downloaderService;

    [ObservableProperty]
    private string _videoUrl = string.Empty;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Models.LogMessage> _logMessages = new();

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _estimatedTimeRemaining = "Estimating...";

    // Window properties for persistence
    public string WindowTitle { get; } = $"YouTube Downloader v{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0"}";
    public double WindowWidth { get; set; } = 800;
    public double WindowHeight { get; set; } = 450;
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;

    [ObservableProperty]
    private bool _enableDependencyLog = true;


    public MainWindowViewModel()
    {
        _downloaderService = new DownloaderService();
        _downloaderService.OutputReceived += OnOutputReceived;
        _downloaderService.ErrorReceived += OnErrorReceived;
        _downloaderService.ProgressChanged += OnProgressChanged;
        _downloaderService.EstimatedTimeRemainingChanged += OnEstimatedTimeRemainingChanged;
        _downloaderService.DebugLogReceived += (s, msg) =>
        {
            if (EnableDependencyLog) AppendLog($"[Service] {msg}");
        };

        // Load settings
        LoadSettings();

        // Initial check
        CheckDependencies();
    }

    private void LoadSettings()
    {
        var settings = Services.SettingsManager.Load();
        VideoUrl = settings.LastVideoUrl ?? string.Empty;
        DownloadPath = settings.LastDownloadPath ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        WindowWidth = settings.WindowWidth;
        WindowHeight = settings.WindowHeight;
        WindowX = settings.WindowX;
        WindowY = settings.WindowY;
        EnableDependencyLog = settings.EnableDependencyLog;
    }

    public void SaveSettings()
    {
        var settings = new Services.AppSettings
        {
            LastVideoUrl = null,
            LastDownloadPath = DownloadPath,
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            WindowX = WindowX,
            WindowY = WindowY,
            EnableDependencyLog = EnableDependencyLog
        };
        Services.SettingsManager.Save(settings);
    }

    [RelayCommand]
    private async Task BrowseAsync(Control? control)
    {
        if (control == null) return;

        var topLevel = TopLevel.GetTopLevel(control);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Download Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            DownloadPath = folders[0].Path.LocalPath;
            SaveSettings(); // Auto-save on change
        }
    }

    private void OnProgressChanged(object? sender, double e)
    {
        ProgressValue = e;
    }

    private void OnEstimatedTimeRemainingChanged(object? sender, TimeSpan? eta)
    {
        EstimatedTimeRemaining = eta.HasValue
            ? $"Estimated remaining: {eta.Value:hh\\:mm\\:ss}"
            : "Estimating...";
    }

    private async void CheckDependencies()
    {
        if (EnableDependencyLog) AppendLog("[CheckDependencies] Starting dependency checks...");

        var ytDlpTask = Task.Run(() => _downloaderService.IsYtDlpInstalledAsync());
        var ffmpegTask = Task.Run(() => _downloaderService.IsFfmpegInstalledAsync());
        var nodeTask = Task.Run(() => _downloaderService.IsNodeInstalledAsync());

        await Task.WhenAll(ytDlpTask, ffmpegTask, nodeTask);

        bool ytDlp = await ytDlpTask;
        bool ffmpeg = await ffmpegTask;
        bool node = await nodeTask;

        if (ytDlp) AppendLog("yt-dlp detected.");
        else
        {
            AppendLog("Error: 'yt-dlp' is not found.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                AppendLog("Please install it via Winget: winget install yt-dlp");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                AppendLog("Please install it via Homebrew: brew install yt-dlp");
            else
                AppendLog("Please install it via your package manager.");
        }

        if (ffmpeg) AppendLog("ffmpeg detected.");
        else
        {
            AppendLog("Warning: 'ffmpeg' is not found. Merging might fail.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                AppendLog("Please install it via Winget: winget install Gyan.FFmpeg");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                AppendLog("Please install it via Homebrew: brew install ffmpeg");
            else
                AppendLog("Please install it via your package manager.");
        }

        if (node) AppendLog("Node.js detected.");
        else
        {
            AppendLog("Warning: 'Node.js' is not found. Some videos might fail to download or be throttled.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                AppendLog("Please install it via Winget: winget install OpenJS.NodeJS");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                AppendLog("Please install it via Homebrew: brew install node");
            else
                AppendLog("Please install it via your package manager.");
        }
    }

    private void OnOutputReceived(object? sender, string e)
    {
        AppendLog(e);
    }

    private void OnErrorReceived(object? sender, string e)
    {
        AppendLog($"[Error/Info] {e}");
    }

    private void AppendLog(string message)
    {
        Avalonia.Media.IBrush color;

        if (message.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("[Error]", StringComparison.OrdinalIgnoreCase))
        {
            color = Avalonia.Media.Brushes.Red;
        }
        else if (message.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("[Warning]", StringComparison.OrdinalIgnoreCase))
        {
            color = Avalonia.Media.Brushes.Yellow;
        }
        else if (message.Contains("Success", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("completed", StringComparison.OrdinalIgnoreCase))
        {
            color = Avalonia.Media.Brushes.LightGreen;
        }
        else if (message.Contains("[Service]") || message.Contains("[CheckDependencies]"))
        {
            color = Avalonia.Media.Brushes.DarkGray;
        }
        else
        {
            color = Avalonia.Media.Brushes.White;
        }

        Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
        {
            LogMessages.Add(new Models.LogMessage(message, color));
            // Limit log size
            if (LogMessages.Count > 1000)
            {
                LogMessages.RemoveAt(0);
            }
        });
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(VideoUrl))
        {
            StatusMessage = "Please enter a valid URL.";
            return;
        }

        if (string.IsNullOrWhiteSpace(DownloadPath))
        {
            DownloadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        SaveSettings(); // Save state before starting

        IsDownloading = true;
        StatusMessage = "Downloading...";
        ProgressValue = 0;
        EstimatedTimeRemaining = "Estimating...";
        LogMessages.Clear(); // Clear log
        AppendLog($"Starting download: {GetSafeUrlForLog(VideoUrl)}");
        AppendLog($"Output Path: {DownloadPath}");

        try
        {
            // Run download in background thread to keep UI responsive
            int exitCode = await Task.Run(() => _downloaderService.DownloadVideoAsync(VideoUrl, DownloadPath));

            if (exitCode == 0)
            {
                StatusMessage = "Download completed successfully!";
                AppendLog("Download completed successfully!");
                ProgressValue = 100;
                EstimatedTimeRemaining = "Estimated remaining: 00:00:00";
            }
            else
            {
                StatusMessage = $"Download failed. Exit code: {exitCode}";
                AppendLog($"Download failed. Exit code: {exitCode}");
                EstimatedTimeRemaining = "Estimating...";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error occurred.";
            AppendLog($"Error: {ex.Message}");
            EstimatedTimeRemaining = "Estimating...";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private static string GetSafeUrlForLog(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return "[invalid-url]";
        }

        return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
    }

    [RelayCommand]
    private void OpenSettings(Control? control)
    {
        if (control == null) return;
        var topLevel = TopLevel.GetTopLevel(control) as Window;
        if (topLevel == null) return;

        var settingsWindow = new Views.SettingsWindow
        {
            DataContext = this
        };
        settingsWindow.ShowDialog(topLevel);
    }

    [RelayCommand]
    private async Task CopyAllLogsAsync(Control? control)
    {
        if (control == null) return;
        var topLevel = TopLevel.GetTopLevel(control);
        if (topLevel?.Clipboard == null) return;

        var fullLog = string.Join(Environment.NewLine, LogMessages.Select(m => m.Text));
        await topLevel.Clipboard.SetTextAsync(fullLog);
    }

    [RelayCommand]
    private void ClearLogs()
    {
        LogMessages.Clear();
    }

    partial void OnEnableDependencyLogChanged(bool value) => SaveSettings();
}
