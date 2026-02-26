using Xui.Core.Actual;
using Xui.Core.Debug;

namespace Xui.Core.Abstract;

/// <summary>
/// Represents an abstract base class for Xui applications.
/// This class is paired at runtime with a platform-specific counterpart,
/// which delegates to actual system APIs on macOS, Windows, Android, etc.
///
/// Users should subclass <see cref="Application"/>, override the <see cref="Start"/> method,
/// and call <see cref="Run"/> to launch the application.
/// </summary>
public abstract class Application : IServiceProvider
{
    public IServiceProvider Context { get; }

    /// <inheritdoc/>
    public virtual object? GetService(Type serviceType) => Context.GetService(serviceType);

    public IRuntime Runtime { get; }

    public List<IDisposable> DisposeQueue { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    public Application(IServiceProvider context)
    {
        this.Context = context!;
        this.Runtime = (IRuntime)this.Context.GetService(typeof(IRuntime))!;
    }

    /// <summary>
    /// Starts the main application loop by delegating to the platform-specific run loop.
    /// This method may block until the application exits,
    /// or may return immediately if the platform bootstraps a runtime loop before instantiating the app delegate.
    /// </summary>
    /// <returns>The application’s exit code.</returns>
    private IRunLoop? runLoop;

    public virtual int Run()
    {
        // this.Runtime.CurrentInstruments.Log(Scope.Application, LevelOfDetail.Essential,
        //     $"Application.Run {this.GetType().Name}");
        this.runLoop = Runtime.CreateRunloop(this);
        return this.runLoop.Run();
    }

    /// <summary>
    /// Requests graceful shutdown of the application's run loop.
    /// On platforms without a blocking run loop (iOS, Browser) this is a no-op.
    /// </summary>
    public virtual void Quit() => this.runLoop?.Quit();

    /// <summary>
    /// Called by the runtime after initialization.
    /// Override this method to set up application state and display the initial UI.
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Raised when the application exits on platforms with a definite exit point.
    /// See <see cref="OnExit"/> for the same constraint on when this fires.
    /// </summary>
    public event Action? Exit;

    /// <summary>
    /// Called by run loops that have a definite exit point (e.g. Win32, macOS) just before
    /// their <see cref="IRunLoop.Run"/> returns. Not called on platforms whose run loop never
    /// exits (iOS, Browser canvas), so do not rely on this for cleanup that must always run.
    /// </summary>
    public virtual void OnExit()
    {
        Exit?.Invoke();
        foreach (var item in DisposeQueue) item.Dispose();
        DisposeQueue.Clear();
    }
}
