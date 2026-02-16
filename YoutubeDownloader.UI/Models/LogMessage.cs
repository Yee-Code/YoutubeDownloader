using Avalonia.Media;

namespace YoutubeDownloader.UI.Models;

public class LogMessage
{
    public string Text { get; }
    public IBrush Color { get; }

    public LogMessage(string text, IBrush color)
    {
        Text = text;
        Color = color;
    }
}
