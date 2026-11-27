using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Apps.LoadTestApp;
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
#endif

var exitCode = new HostBuilder()
    .UseRuntime()
    .ConfigureServices(config => config
        .AddScoped<MainWindow>()
        .AddScoped<Application>())
    .Build()
    .Run<Application>();

#if BROWSER
await browserHost.RunAsync();
#endif
return exitCode;
