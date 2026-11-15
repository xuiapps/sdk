namespace Xui.Core.Abstract.Events;

/// <summary>
/// Describes the phase of an individual touch point during a touch event.
/// </summary>
public enum TouchPhase
{
    /// <summary>
    /// The touch has just started (finger or stylus contacted the surface).
    /// </summary>
    Start,

    /// <summary>
    /// The touch is actively moving across the surface.
    /// </summary>
    Move,

    /// <summary>
    /// The touch has ended (finger or stylus lifted off the surface).
    /// </summary>
    End,

    /// <summary>The system interrupted the contact; it must not complete a tap.</summary>
    Cancel
}
