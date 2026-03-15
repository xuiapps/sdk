namespace Xui.Core.UI;

/// <summary>
/// Controls the visual backdrop material of a popup surface.
/// Mirrors <see cref="Xui.Core.Abstract.IWindow.IDesktopStyle.WindowBackdrop"/>
/// for popup-specific use.
/// </summary>
public enum PopupEffect
{
    /// <summary>
    /// Standard opaque popup with no translucency.
    /// </summary>
    None = 0,

    /// <summary>
    /// Translucent popup using the system's standard popup/popover material.
    /// On macOS this uses <c>NSVisualEffectView</c> with the <c>Popover</c> material,
    /// which on macOS 26+ automatically gains the liquid-glass treatment.
    /// </summary>
    Translucent = 1,
}
