using Avalonia.Controls;
using YoutubeDownloader.UI.ViewModels;

namespace YoutubeDownloader.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is MainWindowViewModel vm)
        {
            // Restore position if valid
            if (vm.WindowX != -1 && vm.WindowY != -1)
            {
                Position = new Avalonia.PixelPoint(vm.WindowX, vm.WindowY);
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext is MainWindowViewModel vm)
        {
            // Update VM properties from Window state before saving
            vm.WindowWidth = Width;
            vm.WindowHeight = Height;
            // Currently Avalonia Window.Position binding is tricky, saving manually
            vm.WindowX = Position.X;
            vm.WindowY = Position.Y;

            vm.SaveSettings();
        }
    }
}