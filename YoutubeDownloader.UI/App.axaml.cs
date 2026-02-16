using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using YoutubeDownloader.UI.Services;
using YoutubeDownloader.UI.ViewModels;
using YoutubeDownloader.UI.Views;

namespace YoutubeDownloader.UI;

public partial class App : Application
{
    private readonly AppBootstrapper _bootstrapper = new();

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
                DataContext = _bootstrapper.CreateMainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnPreferencesClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is MainWindow mainWindow)
        {
            OpenSettingsWindow(mainWindow);
        }
    }

    private static void OpenSettingsWindow(Window owner)
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is MainWindow mainWindow)
        {
            OpenAboutWindow(mainWindow);
        }
    }

    private static void OpenAboutWindow(Window owner)
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(owner);
    }
}
