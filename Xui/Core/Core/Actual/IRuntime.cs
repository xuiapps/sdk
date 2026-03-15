namespace Xui.Core.Actual;

/// <summary>
/// Provides a platform-specific implementation of the Xui runtime,
/// responsible for creating and connecting abstract application components to their actual counterparts.
///
/// This interface acts as a bridge between the platform-independent core and the underlying OS-specific APIs
/// (e.g., Win32, Cocoa, UIKit), enabling rendering, windowing, and event dispatch.
/// </summary>
public interface IRuntime
{
    /// <summary>
    /// Gets the main thread dispatcher for scheduling UI work.
    /// Used to marshal execution onto the main thread for layout, input, and rendering.
    /// </summary>
    IDispatcher MainDispatcher { get; }

    /// <summary>
    /// Creates a platform-specific window that is bound to the given abstract window definition.
    /// </summary>
    /// <param name="windowAbstract">The abstract window definition provided by user code.</param>
    /// <returns>A concrete window implementation for the current platform.</returns>
    Actual.IWindow CreateWindow(Abstract.IWindow windowAbstract);

    /// <summary>
    /// Creates a platform-specific run loop associated with the given abstract application.
    /// The returned run loop is responsible for managing the application's execution lifecycle.
    /// </summary>
    /// <param name="applicationAbstract">The abstract application instance defined by user code.</param>
    /// <returns>A platform-specific run loop instance.</returns>
    IRunLoop CreateRunloop(Abstract.Application applicationAbstract);
}
