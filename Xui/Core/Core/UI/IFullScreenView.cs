namespace Xui.Core.UI;

/// <summary>Declares whether root content supplies an opaque background for the entire window.</summary>
/// <remarks>
/// This is a coverage promise, not a layout hint. Return true only when every draw
/// covers the whole display area with opaque pixels. Hosts may skip their clear.
/// Wrappers must forward the promise explicitly, and changes must invalidate the view.
/// </remarks>
public interface IFullScreenView
{
    /// <summary>Whether the current state paints the whole display area opaquely.</summary>
    bool DrawsOpaqueFullScreen { get; }
}
