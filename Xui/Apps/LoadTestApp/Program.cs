using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Apps.LoadTestApp;
using Xui.Core.DI;

return new HostBuilder()
    .UseRuntime()
    .ConfigureServices(config => config
        .AddScoped<MainWindow>()
        .AddScoped<Application>())
    .Build()
    .Run<Application>();
