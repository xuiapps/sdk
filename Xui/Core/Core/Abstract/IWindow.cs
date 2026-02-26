using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;

namespace Xui.Core.Abstract;

/// <summary>
/// Defines the abstract interface for a platform window in Xui.
/// This surface hosts rendering, input handling, and layout updates.
/// </summary>
/// <remarks>
/// Implementations of this interface bridge platform-specific <c>Actual</c> windowing
/// with the framework’s abstract layer. It is used both for physical windows (e.g. desktop apps)
/// and virtual windows (e.g. emulator windows).
/// </remarks>
public partial interface IWindow
{
    /// <summary>
    /// Gets or sets the total visible area of the window, including content that may
    /// be obscured by hardware cutouts, rounded corners, or system UI overlays.
    /// </summary>
    /// <remarks>
    /// Used by the layout system to determine the full available size.
    /// </remarks>
    public Rect DisplayArea { get; set; }

    /// <summary>
    /// Gets or sets the "safe" area of the window, excluding obstructions like notches
    /// or status bars. Important UI elements should be placed within this area.
    /// </summary>
    /// <remarks>
    /// Especially relevant on mobile devices and in emulator scenarios.
    /// </remarks>
    public Rect SafeArea { get; set; }

    /// <summary>
    /// Gets or sets the corner radius of the physical screen, in logical pixels.
    /// Used to offset UI elements (such as scrollbar indicators) away from rounded screen edges.
    /// Zero on desktop platforms where the window has sharp corners.
    /// </summary>
    public nfloat ScreenCornerRadius { get; set; }

    /// <summary>
    /// Invoked when the window is closed and cleanup should occur.
    /// </summary>
    void Closed();

    /// <summary>
    /// Invoked before the window closes. Returning <c>false</c> can cancel the closure.
    /// </summary>
    /// <returns><c>true</c> if the window may close; otherwise, <c>false</c>.</returns>
    bool Closing();

    /// <summary>
    /// Invoked once per frame to propagate animation timing information.
    /// </summary>
    /// <param name="animationFrame">Timing details for the current animation frame.</param>
    void OnAnimationFrame(ref FrameEventRef animationFrame);

    /// <summary>
    /// Invoked when the mouse is moved within the window.
    /// </summary>
    /// <param name="evRef">The mouse movement event data.</param>
    void OnMouseMove(ref MouseMoveEventRef evRef);

    /// <summary>
    /// Invoked when a mouse button is pressed within the window.
    /// </summary>
    /// <param name="evRef">The mouse down event data.</param>
    void OnMouseDown(ref MouseDownEventRef evRef);

    /// <summary>
    /// Invoked when a mouse button is released within the window.
    /// </summary>
    /// <param name="evRef">The mouse up event data.</param>
    void OnMouseUp(ref MouseUpEventRef evRef);

    /// <summary>
    /// Invoked when the scroll wheel is used within the window.
    /// </summary>
    /// <param name="evRef">The scroll wheel event data.</param>
    void OnScrollWheel(ref ScrollWheelEventRef evRef);

    /// <summary>
    /// Invoked when touch input occurs within the window.
    /// </summary>
    /// <param name="touchEventRef">The touch event data.</param>
    void OnTouch(ref TouchEventRef touchEventRef);

    /// <summary>
    /// Invoked during the render phase of the UI lifecycle.
    /// Responsible for triggering layout and visual updates.
    /// </summary>
    /// <param name="render">The render event data, including target rect and frame info.</param>
    void Render(ref RenderEventRef render);

    /// <summary>
    /// Invoked when the system requests a hit test for window interaction.
    /// Allows the app to indicate whether a region is draggable, resizable, etc.
    /// </summary>
    /// <param name="evRef">The hit test event containing pointer position and window bounds.</param>
    void WindowHitTest(ref WindowHitTestEventRef evRef);

    /// <summary>
    /// Invoked when a key is pressed.
    /// </summary>
    /// <param name="e">The keyboard event data.</param>
    void OnKeyDown(ref KeyEventRef e);

    /// <summary>
    /// Invoked when a character is input (after keyboard translation).
    /// </summary>
    /// <param name="e">The keyboard event data with the translated character.</param>
    void OnChar(ref KeyEventRef e);
}
