using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Core.Abstract;

namespace Xui.Core.DI;

public static class HostExtensions
{
    public static int Run<TApplication>(this IHost host)
        where TApplication : Application
    {
        host.Start();
        var application = host.Services.GetRequiredService<TApplication>();
        application.DisposeQueue += host;
        return application.Run();
    }
}

public static class ApplicationExtensions
{
    /// <summary>
    /// Creates a DI scope, resolves <typeparamref name="TWindow"/> from it, and shows it.
    /// The scope is added to the window's <see cref="Window.DisposeQueue"/> so it is disposed
    /// automatically when the window closes (assuming <see cref="Window.DestroyOnClose"/> is true).
    /// </summary>
    public static void CreateAndShowOnce<TWindow>(this Application app)
        where TWindow : Window
    {
        var scope = app.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<TWindow>();
        window.DisposeQueue += scope;
        window.Show();
    }
}
