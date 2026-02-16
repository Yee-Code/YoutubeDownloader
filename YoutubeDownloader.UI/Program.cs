using Avalonia;
using System;
using System.Threading.Tasks;

namespace YoutubeDownloader.UI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length > 0)
            {
                await CliRunner.RunAsync(args);
                return 0;
            }
            else
            {
                return BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("crash.log", ex.ToString());
            return 1;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
