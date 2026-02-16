using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using YoutubeDownloader.UI.ViewModels;
using YoutubeDownloader.UI.Views;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnPreferencesClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is MainWindow mainWindow)
        {
            if (mainWindow.DataContext is MainWindowViewModel viewModel)
            {
                OpenSettingsWindow(mainWindow);
            }
        }
    }

    private void OpenSettingsWindow(Window owner)
    {
        if (owner.DataContext is MainWindowViewModel viewModel)
        {
            var settingsWindow = new SettingsWindow
            {
                DataContext = viewModel
            };
            settingsWindow.ShowDialog(owner);
        }
    }
    private void OnAboutClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is MainWindow mainWindow)
        {
            OpenAboutWindow(mainWindow);
        }
    }

    private void OpenAboutWindow(Window owner)
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(owner);
    }
}
