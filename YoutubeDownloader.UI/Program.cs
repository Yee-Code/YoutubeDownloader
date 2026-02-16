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
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0)
            {
                // Synchronously run CLI mode
                CliRunner.RunAsync(args).GetAwaiter().GetResult();
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
            var logMessage = $"[{DateTime.Now}] Crash Report:{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
            System.IO.File.AppendAllText("crash.log", logMessage);
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
