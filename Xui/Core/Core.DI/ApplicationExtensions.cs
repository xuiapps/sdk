using Microsoft.Extensions.DependencyInjection;
using Xui.Core.Abstract;

namespace Xui.Core.DI;

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
        window.DestroyOnClose = true;
        window.DisposeQueue.Add(scope);
        window.Show();
    }

    /// <summary>
    /// Like <see cref="CreateAndShowOnce{TWindow}"/>, but also calls <see cref="Application.Quit"/>
    /// after the window closes and its scope is disposed. Use this for the application's main window
    /// so that closing it terminates the process.
    /// </summary>
    public static void CreateAndShowMainWindowOnce<TWindow>(this Application app)
        where TWindow : Window
    {
        var scope = app.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<TWindow>();
        window.DestroyOnClose = true;
        window.DisposeQueue.Add(scope);
        window.DisposeQueue.Add(new ActionDisposable(app.Quit));
        window.Show();
    }

    sealed class ActionDisposable(Action action) : IDisposable
    {
        public void Dispose()
        {
            action();
        }
    }
}
