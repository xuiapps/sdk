using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewBlankApp;
using Xui.Core.DI;

return new HostBuilder()
    .UseRuntime()
    .ConfigureServices(config => config
        .AddScoped<MainWindow>()
        .AddScoped<Application>())
    .Build()
    .Run<Application>();
