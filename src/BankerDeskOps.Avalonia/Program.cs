using Avalonia;

namespace BankerDeskOps.Avalonia;

/// <summary>
/// Application entry point. Must remain a standalone class so Avalonia's
/// designer and hot-reload tooling can locate the AppBuilder.
/// </summary>
internal sealed class Program
{
    // Initialization code. Do NOT use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: the GUI
    // thread has not yet been established.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Called by Avalonia tooling (designer / hot-reload) to create the AppBuilder.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()   // macOS → native Quartz; Windows → Win32; Linux → X11/Wayland
                  .WithInterFont()
                  .LogToTrace();
}
