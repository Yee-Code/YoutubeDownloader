using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core;

namespace YoutubeDownloader.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DownloaderService _downloaderService;

    [ObservableProperty]
    private string _videoUrl = string.Empty;

    [ObservableProperty]
    private string _logOutput = string.Empty;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainWindowViewModel()
    {
        _downloaderService = new DownloaderService();
        _downloaderService.OutputReceived += OnOutputReceived;
        _downloaderService.ErrorReceived += OnErrorReceived;

        // Initial check
        CheckDependencies();
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

        IsDownloading = true;
        StatusMessage = "Downloading...";
        LogOutput = string.Empty; // Clear log
        AppendLog($"Starting download: {VideoUrl}");

        try
        {
            int exitCode = await _downloaderService.DownloadVideoAsync(VideoUrl);

            if (exitCode == 0)
            {
                StatusMessage = "Download completed successfully!";
                AppendLog("Download completed successfully!");
            }
            else
            {
                StatusMessage = $"Download failed. Exit code: {exitCode}";
                AppendLog($"Download failed. Exit code: {exitCode}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error occurred.";
            AppendLog($"Error: {ex.Message}");
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
