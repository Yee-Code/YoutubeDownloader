using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Abstractions;
using YoutubeDownloader.Core.Models;
using YoutubeDownloader.Core.Services;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDependencyService _dependencyService;
    private readonly IDownloadClient _downloadClient;

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
    private bool _isPaused;

    private System.Threading.CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string _estimatedTimeRemaining = "Estimating...";

    public string WindowTitle { get; } = $"YouTube Downloader v{AppInfo.GetVersion()}";

    [ObservableProperty]
    private double _windowWidth = 800;

    [ObservableProperty]
    private double _windowHeight = 450;

    [ObservableProperty]
    private int _windowX = -1;

    [ObservableProperty]
    private int _windowY = -1;

    [ObservableProperty]
    private bool _enableDependencyLog = true;

    public MainWindowViewModel()
        : this(CreateDefaultServices())
    {
    }

    private MainWindowViewModel((IDependencyService DependencyService, IDownloadClient DownloadClient) services)
        : this(services.DependencyService, services.DownloadClient)
    {
    }

    public MainWindowViewModel(IDependencyService dependencyService, IDownloadClient downloadClient)
    {
        _dependencyService = dependencyService;
        _downloadClient = downloadClient;

        LoadSettings();
        _ = CheckDependenciesAsync();
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
        if (control == null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(control);
        if (topLevel == null)
        {
            return;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Download Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            DownloadPath = folders[0].Path.LocalPath;
            SaveSettings();
        }
    }

    private async Task CheckDependenciesAsync()
    {
        if (EnableDependencyLog)
        {
            AppendLog("[DependencyCheck] Starting checks...");
        }

        var dependencies = await _dependencyService.CheckAsync().ConfigureAwait(false);

        foreach (var dependency in dependencies)
        {
            if (dependency.IsInstalled)
            {
                AppendLog($"{dependency.ExecutableName} detected.");
                continue;
            }

            var severityPrefix = dependency.Type == DependencyType.YtDlp ? "Error" : "Warning";
            AppendLog($"{severityPrefix}: '{dependency.ExecutableName}' is not found.");

            foreach (var hint in DependencyGuidance.GetInstallHints(dependency.Type))
            {
                AppendLog(hint);
            }
        }
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
        else if (message.Contains("[DependencyCheck]", StringComparison.OrdinalIgnoreCase))
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

        SaveSettings();

        IsDownloading = true;
        IsPaused = false;
        StatusMessage = "Downloading...";
        ProgressValue = 0;
        EstimatedTimeRemaining = "Estimating...";
        LogMessages.Clear();

        await StartDownloadAsync();
    }

    private async Task StartDownloadAsync()
    {
        _cancellationTokenSource = new System.Threading.CancellationTokenSource();

        AppendLog($"Starting download: {GetSafeUrlForLog(VideoUrl)}");
        AppendLog($"Output Path: {DownloadPath}");



        try
        {
            var result = await _downloadClient.DownloadAsync(
                new DownloadRequest(VideoUrl, DownloadPath),
                onOutput: AppendLog,
                onError: line => AppendLog($"[Error/Info] {line}"),
                onProgress: progress =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = progress.Percentage;
                        EstimatedTimeRemaining = progress.EstimatedTimeRemaining.HasValue
                            ? $"Estimated remaining: {progress.EstimatedTimeRemaining.Value:hh\\:mm\\:ss}"
                            : "Estimating...";
                    });
                },
                onDebug: message =>
                {
                    if (EnableDependencyLog)
                    {
                        AppendLog($"[Service] {message}");
                    }
                },
                cancellationToken: _cancellationTokenSource.Token);

            if (result.IsSuccess)
            {
                StatusMessage = "Download completed successfully!";
                AppendLog("Download completed successfully!");
                ProgressValue = 100;
                EstimatedTimeRemaining = "Estimated remaining: 00:00:00";
            }
            else
            {
                StatusMessage = $"Download failed. Exit code: {result.ExitCode}";
                AppendLog($"Download failed. Exit code: {result.ExitCode}");
                EstimatedTimeRemaining = "Estimating...";
            }
        }
        catch (OperationCanceledException)
        {
            if (IsPaused)
            {
                StatusMessage = "Paused";
                AppendLog("Download paused.");
            }
            else
            {
                StatusMessage = "Cancelled";
                AppendLog("Download cancelled.");
                IsDownloading = false;
                ProgressValue = 0;
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
            if (!IsPaused)
            {
                IsDownloading = false;
            }

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand]
    private void Pause()
    {
        if (IsDownloading && !IsPaused)
        {
            IsPaused = true;
            _cancellationTokenSource?.Cancel();
        }
    }

    [RelayCommand]
    private async Task ResumeAsync()
    {
        if (IsDownloading && IsPaused)
        {
            IsPaused = false;
            StatusMessage = "Resuming...";
            AppendLog("Resuming download...");
            await StartDownloadAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsDownloading)
        {
            if (IsPaused)
            {
                IsPaused = false;
                IsDownloading = false;
                StatusMessage = "Cancelled";
                AppendLog("Download cancelled.");
                ProgressValue = 0;
                EstimatedTimeRemaining = "Estimating...";
            }
            else
            {
                IsPaused = false; // Ensure we don't convert to paused state
                _cancellationTokenSource?.Cancel();
            }
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
        if (control == null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(control) as Window;
        if (topLevel == null)
        {
            return;
        }

        var settingsWindow = new Views.SettingsWindow
        {
            DataContext = this
        };

        settingsWindow.ShowDialog(topLevel);
    }

    [RelayCommand]
    private async Task CopyAllLogsAsync(Control? control)
    {
        if (control == null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(control);
        if (topLevel?.Clipboard == null)
        {
            return;
        }

        var fullLog = string.Join(Environment.NewLine, LogMessages.Select(m => m.Text));
        await topLevel.Clipboard.SetTextAsync(fullLog);
    }

    [RelayCommand]
    private void ClearLogs()
    {
        LogMessages.Clear();
    }

    partial void OnEnableDependencyLogChanged(bool value) => SaveSettings();

    [RelayCommand]
    private void CloseWindow(Window? window)
    {
        window?.Close();
    }

    private static (IDependencyService DependencyService, IDownloadClient DownloadClient) CreateDefaultServices()
    {
        var bootstrapper = new Services.AppBootstrapper();
        return (bootstrapper.DependencyService, bootstrapper.DownloadClient);
    }
}
