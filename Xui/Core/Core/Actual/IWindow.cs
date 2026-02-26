using System;
using Xui.Core.Canvas;

namespace Xui.Core.Actual;

/// <summary>
/// Represents a platform-specific window implementation used by the Xui runtime.
/// Each platform (e.g., Windows, macOS, iOS) must provide an implementation of this interface
/// to manage window lifecycle, rendering, and input.
///
/// This interface is typically paired with an abstract window in the Xui framework,
/// and is not used directly by application developers.
/// </summary>
public interface IWindow
{
    /// <summary>
    /// Gets or sets the window title, where supported by the platform (e.g., desktop).
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Displays the window to the user. This may include making it visible, entering the main loop,
    /// or attaching it to the application's view hierarchy, depending on the platform.
    /// </summary>
    void Show();

    /// <summary>
    /// Closes and destroys the platform window. Called when the abstract window is disposed
    /// programmatically rather than through a user-initiated close gesture.
    /// The default implementation is a no-op for platforms that do not support imperative close.
    /// </summary>
    void Close() { }

    /// <summary>
    /// Requests a redraw of the window surface.
    /// The platform should trigger a paint or render callback as soon as possible.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Gets or sets whether the window currently requires keyboard input focus.
    /// Platforms may use this to show or hide on-screen keyboards.
    /// </summary>
    bool RequireKeyboard { get; set; }

    /// <summary>
    /// Gets a lightweight text measure context for hit-testing text positions
    /// during pointer events. Returns null on platforms that do not support it.
    /// </summary>
    ITextMeasureContext? TextMeasureContext => null;

    /// <summary>
    /// Returns platform-provided services for this window (e.g. <see cref="IImagePipeline"/>).
    /// Called by the abstract <see cref="Xui.Core.Abstract.Window"/> after exhausting its own
    /// DI service provider. Implementations must never call back into the abstract window.
    /// </summary>
    object? GetService(Type serviceType) => null;
}
