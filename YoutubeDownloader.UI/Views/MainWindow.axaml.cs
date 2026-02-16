using Avalonia.Controls;
using YoutubeDownloader.UI.ViewModels;

namespace YoutubeDownloader.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.LogMessages.CollectionChanged += OnLogMessagesChanged;
            // Initialize existing logs if any
            RebuildLogs(vm.LogMessages);
        }
    }

    private void OnLogMessagesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        {
            LogTextBlock.Inlines!.Clear();
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (Models.LogMessage msg in e.NewItems)
            {
                AddLogMessage(msg);
            }
            // Auto scroll
            LogScrollViewer.ScrollToEnd();
        }
    }

    private void RebuildLogs(System.Collections.Generic.IEnumerable<Models.LogMessage> messages)
    {
        LogTextBlock.Inlines!.Clear();
        foreach (var msg in messages)
        {
            AddLogMessage(msg);
        }
        LogScrollViewer.ScrollToEnd();
    }

    private void AddLogMessage(Models.LogMessage msg)
    {
        var run = new Avalonia.Controls.Documents.Run(msg.Text + System.Environment.NewLine)
        {
            Foreground = msg.Color
        };
        LogTextBlock.Inlines!.Add(run);
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

    private void OnCloseClicked(object? sender, System.EventArgs e)
    {
        Close();
    }
}