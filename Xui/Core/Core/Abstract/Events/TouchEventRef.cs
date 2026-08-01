namespace Xui.Core.Abstract.Events;

/// <summary>
/// Represents a platform-level input event containing one or more touch points,
/// typically originating from a touchscreen or trackpad gesture.
/// </summary>
/// <remarks>
/// This event is dispatched by the <c>Actual</c> window and forwarded
/// to the <c>Abstract</c> layer for routing through the view hierarchy.
/// It includes all active touch points for the current frame, allowing
/// gesture recognition, hit testing, and view interaction logic.
/// </remarks>
public ref struct TouchEventRef
{
    /// <summary>
    /// A span of all current touch points involved in this event.
    /// </summary>
    public readonly ReadOnlySpan<Touch> Touches;

    /// <summary>Text measurement supplied by the window for precision caret hit-testing.</summary>
    public Xui.Core.Canvas.ITextMeasureContext? TextMeasure;

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchEventRef"/> struct
    /// with the provided set of touch points.
    /// </summary>
    /// <param name="touches">A span of active touch data.</param>
    public TouchEventRef(ReadOnlySpan<Touch> touches)
    {
        Touches = touches;
        TextMeasure = null;
    }
}
