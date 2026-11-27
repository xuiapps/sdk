using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages.WindowModes.MacOS;
using Xui.Core.DI;

#if BROWSER
var browserHost = Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder.CreateDefault(args).Build();
#if XUI_BROWSER_AOT
await Xui.Runtime.Browser.Actual.BrowserPlatform.InitializeAsync(".NET 10 · AOT");
#elif DEBUG
await Xui.Runtime.Browser.Actual.BrowserPlatform.InitializeAsync(".NET 10 · interpreter · Debug");
#else
await Xui.Runtime.Browser.Actual.BrowserPlatform.InitializeAsync(".NET 10 · interpreter / Jiterpreter · Release");
#endif
TestAppBootstrap.Run();
await browserHost.RunAsync();
return 0;
#elif !ANDROID
return TestAppBootstrap.Run();
#endif

public static class TestAppBootstrap
{
    public static int Run()
    {
        return new HostBuilder()
            .UseRuntime()
            .ConfigureServices(config => config
                .AddScoped<MainWindow>()
                .AddTransient<StandardDesktopWindow>()
                .AddTransient<TitledUnifiedCompactDesktopWindow>()
                .AddTransient<TitledUnifiedDesktopWindow>()
                .AddTransient<UntitledDesktopWindow>()
                .AddTransient<UntitledUnifiedCompactDesktopWindow>()
                .AddTransient<UntitledUnifiedDesktopWindow>()
                .AddTransient<TitledUnifiedCompactAcrylicDesktopWindow>()
                .AddTransient<UntitledUnifiedGlassDesktopWindow>()
                .AddTransient<UntitledUnifiedAcrylicDesktopWindow>()
                .AddTransient<XuiSDKStyleDesktopWindow>()
                .AddTransient<UntitledGlassDesktopWindow>()
                .AddTransient<UntitledUnifiedCompactGlassDesktopWindow>()
                .AddTransient<TransparentDesktopWindow>()
                .AddTransient<TransparentUnifiedCompactDesktopWindow>()
                .AddTransient<TransparentUnifiedDesktopWindow>()
                .AddTransient<BorderlessDesktopWindow>()
                .AddTransient<BorderlessUnifiedCompactDesktopWindow>()
                .AddTransient<BorderlessUnifiedDesktopWindow>()
                .AddScoped<Application>())
            .Build()
            .Run<Application>();
    }
}
