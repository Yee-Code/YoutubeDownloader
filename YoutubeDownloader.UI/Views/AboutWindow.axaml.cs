using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.UI.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var versionText = this.FindControl<TextBlock>("VersionText");
        if (versionText != null)
        {
            versionText.Text = $"Version {AppInfo.GetVersion()}";
        }
    }

    private void OnOkClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void OnCloseClicked(object? sender, System.EventArgs e)
    {
        Close();
    }
}
