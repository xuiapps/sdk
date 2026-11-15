using Xui.Core.Math2D;

namespace Xui.Core.Actual;

/// <summary>
/// Optional, window-scoped native precision-selection UI. Resolve through the view's
/// service provider; absence (or <see cref="IsSupported"/> false) means no native loupe.
/// This presents rendered content only; the focused view owns text and selection.
/// </summary>
public interface ITextMagnifier
{
    /// <summary>Whether the running OS supports the native magnifier.</summary>
    bool IsSupported { get; }

    /// <summary>
    /// Begins or updates a precision-selection gesture. Both the touch position and
    /// caret bounds are in logical window coordinates, after all view transforms.
    /// Call on the UI thread, only from the focused selection owner.
    /// </summary>
    void Show(Point position, Rect caret);

    /// <summary>
    /// Ends the gesture and releases native state. Safe to call repeatedly.
    /// Call on release, cancellation, loss of capture/focus, or removal of the owner.
    /// </summary>
    void Hide();
}
