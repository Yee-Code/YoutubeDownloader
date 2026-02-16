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
        var settings = Services.SettingsManager.Load();

        var settingsWindow = new Window
        {
            Title = "Preferences",
            Width = 550,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Icon = owner.Icon,
            TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur],
            Background = Brushes.Transparent,
            ExtendClientAreaToDecorationsHint = true
        };

        var rootPanel = new Panel();

        var acrylicBorder = new ExperimentalAcrylicBorder
        {
            IsHitTestVisible = false,
            Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintColor = Color.FromRgb(0, 0, 0),
                TintOpacity = 1,
                MaterialOpacity = 0.65
            }
        };
        rootPanel.Children.Add(acrylicBorder);

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        rootPanel.Children.Add(scrollViewer);

        var content = new StackPanel
        {
            Margin = new Thickness(25),
            Spacing = 15
        };
        scrollViewer.Content = content;

        content.Children.Add(new TextBlock
        {
            Text = "Preferences",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 5, 0, 10)
        });

        var pathPanel = new StackPanel { Spacing = 5 };
        pathPanel.Children.Add(new TextBlock { Text = "Default Download Path:", FontWeight = FontWeight.SemiBold });

        var pathTextBox = new TextBox
        {
            Text = settings.LastDownloadPath
                   ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Watermark = "Download path..."
        };
        pathPanel.Children.Add(pathTextBox);
        content.Children.Add(pathPanel);

        var sizePanel = new StackPanel { Spacing = 10 };
        sizePanel.Children.Add(new TextBlock { Text = "Window Size:", FontWeight = FontWeight.SemiBold });

        var sizeGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 15
        };

        var widthPanel = new StackPanel { Spacing = 3 };
        widthPanel.Children.Add(new TextBlock { Text = "Width:" });
        var widthNumeric = new NumericUpDown
        {
            Value = (decimal)settings.WindowWidth,
            Minimum = 400,
            Maximum = 3840,
            Increment = 10,
            FormatString = "F0"
        };
        widthPanel.Children.Add(widthNumeric);
        Grid.SetColumn(widthPanel, 0);
        sizeGrid.Children.Add(widthPanel);

        var heightPanel = new StackPanel { Spacing = 3 };
        heightPanel.Children.Add(new TextBlock { Text = "Height:" });
        var heightNumeric = new NumericUpDown
        {
            Value = (decimal)settings.WindowHeight,
            Minimum = 300,
            Maximum = 2160,
            Increment = 10,
            FormatString = "F0"
        };
        heightPanel.Children.Add(heightNumeric);
        Grid.SetColumn(heightPanel, 1);
        sizeGrid.Children.Add(heightPanel);

        sizePanel.Children.Add(sizeGrid);
        content.Children.Add(sizePanel);

        var positionPanel = new StackPanel { Spacing = 10 };
        positionPanel.Children.Add(new TextBlock { Text = "Window Position:", FontWeight = FontWeight.SemiBold });

        var posGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 15
        };

        var xPanel = new StackPanel { Spacing = 3 };
        xPanel.Children.Add(new TextBlock { Text = "X Position:" });
        var xNumeric = new NumericUpDown
        {
            Value = settings.WindowX,
            Minimum = -1m,
            Maximum = 7680m,
            Increment = 10m,
            FormatString = "F0"
        };
        xPanel.Children.Add(xNumeric);
        Grid.SetColumn(xPanel, 0);
        posGrid.Children.Add(xPanel);

        var yPanel = new StackPanel { Spacing = 3 };
        yPanel.Children.Add(new TextBlock { Text = "Y Position:" });
        var yNumeric = new NumericUpDown
        {
            Value = settings.WindowY,
            Minimum = -1m,
            Maximum = 4320m,
            Increment = 10m,
            FormatString = "F0"
        };
        yPanel.Children.Add(yNumeric);
        Grid.SetColumn(yPanel, 1);
        posGrid.Children.Add(yPanel);

        positionPanel.Children.Add(posGrid);

        positionPanel.Children.Add(new TextBlock
        {
            Text = "(-1 means default/center position)",
            FontSize = 11,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 5, 0, 0)
        });

        content.Children.Add(positionPanel);

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var saveButton = new Button
        {
            Content = "Save",
            IsDefault = true
        };
        saveButton.Click += (s, e) =>
        {
            try
            {
                settings.LastDownloadPath = pathTextBox.Text;
                settings.WindowWidth = (double)(widthNumeric.Value ?? 800);
                settings.WindowHeight = (double)(heightNumeric.Value ?? 450);
                settings.WindowX = (int)(xNumeric.Value ?? -1);
                settings.WindowY = (int)(yNumeric.Value ?? -1);

                Services.SettingsManager.Save(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save settings error: {ex.Message}");
            }
            finally
            {
                settingsWindow.Close();
            }
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            IsCancel = true
        };
        cancelButton.Click += (s, e) => settingsWindow.Close();

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(saveButton);
        content.Children.Add(buttonPanel);

        settingsWindow.Content = rootPanel;
        settingsWindow.ShowDialog(owner);
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
        var version = AppInfo.GetVersion();

        var aboutWindow = new Window
        {
            Title = "About",
            Width = 450,
            Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Icon = owner.Icon,
            TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur],
            Background = Brushes.Transparent,
            ExtendClientAreaToDecorationsHint = true
        };

        var rootPanel = new Panel();

        var acrylicBorder = new ExperimentalAcrylicBorder
        {
            IsHitTestVisible = false,
            Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintColor = Color.FromRgb(0, 0, 0),
                TintOpacity = 1,
                MaterialOpacity = 0.65
            }
        };
        rootPanel.Children.Add(acrylicBorder);

        var content = new StackPanel
        {
            Margin = new Thickness(30),
            Spacing = 15,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        rootPanel.Children.Add(content);

        content.Children.Add(new TextBlock
        {
            Text = "⬇️",
            FontSize = 48,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        content.Children.Add(new TextBlock
        {
            Text = "YouTube Downloader",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        content.Children.Add(new TextBlock
        {
            Text = $"Version {version}",
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        content.Children.Add(new Border
        {
            Height = 1,
            Background = Brushes.LightGray,
            Margin = new Thickness(0, 10)
        });

        content.Children.Add(new TextBlock
        {
            Text = "A simple and efficient YouTube video downloader powered by yt-dlp.",
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        });

        var okButton = new Button
        {
            Content = "OK",
            IsDefault = true,
            IsCancel = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 15, 0, 0),
            Padding = new Thickness(20, 8)
        };
        okButton.Click += (s, e) => aboutWindow.Close();

        content.Children.Add(okButton);

        aboutWindow.Content = rootPanel;
        aboutWindow.ShowDialog(owner);
    }
}
