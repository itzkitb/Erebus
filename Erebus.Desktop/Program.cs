using Photino.NET;
using Photino.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Erebus.Infrastructure;
using Erebus.App.Shared.State;
using Erebus.App.Shared.Components;
using Erebus.Desktop.Services;
using Erebus.Core.Interfaces;

namespace Erebus.Desktop;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Create local http server for Blazor
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

        // Setup DI container
        appBuilder.Services.AddErebusInfrastructure();
        appBuilder.Services.AddSingleton<VaultSessionState>();
        appBuilder.Services.AddSingleton<IClipboardService, PhotinoClipboardService>();
        appBuilder.Services.AddSingleton<ITimerService, PhotinoTimerService>();

        // Setup root component
        appBuilder.RootComponents.Add<Erebus.App.Shared.Components.App>("#app");

        // Create Photino window
        var app = appBuilder.Build();
        app.MainWindow
            .SetTitle("Erebus")
            .SetMinWidth(800)
            .SetMinHeight(600)
            .SetWidth(1200)
            .SetHeight(800)
            .RegisterFocusInHandler((_, _) => 
                app.Services.GetRequiredService<ITimerService>().ResetTimer());

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Console.Error.WriteLine($"Unhandled exception: {args.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Console.Error.WriteLine($"Unobserved task exception: {args.Exception}");
            args.SetObserved();
        };

        // Run application
        app.Run();
    }
}
