using System;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// A transient popup surface acquired via <c>GetService&lt;IPopup&gt;()</c>.
/// Each call returns a new instance backed by a platform-specific child window.
/// </summary>
/// <remarks>
/// Usage pattern (similar to <see cref="Xui.Core.Canvas.IImage"/>):
/// <code>
/// var popup = this.GetService&lt;IPopup&gt;();
/// popup.Show(content, anchorRect, PopupPlacement.Below);
/// </code>
/// The popup auto-dismisses when the user clicks outside.
/// Disposing the popup closes it if still visible.
/// </remarks>
public interface IPopup : IDisposable
{
    /// <summary>
    /// Shows the popup with the given content, positioned relative to
    /// <paramref name="anchorRect"/> (in the owning window's coordinate space).
    /// </summary>
    /// <param name="content">The view to display inside the popup.</param>
    /// <param name="anchorRect">
    /// The rect of the triggering element in window coordinates.
    /// The platform uses this to position the popup so that (for example)
    /// a selected item aligns with the anchor text.
    /// </param>
    /// <param name="placement">Preferred placement direction.</param>
    /// <param name="size">
    /// Desired popup size. If <c>null</c>, the popup measures the content to determine size.
    /// </param>
    void Show(View content, Rect anchorRect, PopupPlacement placement = PopupPlacement.Below, Size? size = null, PopupEffect effect = PopupEffect.None);

    /// <summary>Closes the popup if visible.</summary>
    void Close();

    /// <summary>Whether the popup is currently visible.</summary>
    bool IsVisible { get; }

    /// <summary>Raised when the popup is dismissed (click-outside, explicit close, or parent window close).</summary>
    event Action? Closed;
}
