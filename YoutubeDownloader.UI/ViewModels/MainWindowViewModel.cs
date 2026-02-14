using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace YoutubeDownloader.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DownloaderService _downloaderService;

    [ObservableProperty]
    private string _videoUrl = string.Empty;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private string _logOutput = string.Empty;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _estimatedTimeRemaining = "Estimating...";

    // Window properties for persistence
    public double WindowWidth { get; set; } = 800;
    public double WindowHeight { get; set; } = 450;
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;

    public MainWindowViewModel()
    {
        _downloaderService = new DownloaderService();
        _downloaderService.OutputReceived += OnOutputReceived;
        _downloaderService.ErrorReceived += OnErrorReceived;
        _downloaderService.ProgressChanged += OnProgressChanged;
        _downloaderService.EstimatedTimeRemainingChanged += OnEstimatedTimeRemainingChanged;

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
            WindowY = WindowY
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
        bool ytDlp = await _downloaderService.IsYtDlpInstalledAsync();
        bool ffmpeg = await _downloaderService.IsFfmpegInstalledAsync();

        if (!ytDlp)
        {
            AppendLog("Error: 'yt-dlp' is not found. Please install it via Homebrew: brew install yt-dlp");
        }
        if (!ffmpeg)
        {
            AppendLog("Warning: 'ffmpeg' is not found. Merging might fail.");
        }

        if (ytDlp) AppendLog("yt-dlp detected.");
        if (ffmpeg) AppendLog("ffmpeg detected.");
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
        // Limit log size to prevent UI lag
        if (LogOutput.Length > 10000)
        {
            LogOutput = LogOutput.Substring(LogOutput.Length - 5000);
        }
        LogOutput += $"{message}\n";
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
        LogOutput = string.Empty; // Clear log
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
}
