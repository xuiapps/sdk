namespace Xui.Core.Actual;

/// <summary>Foreground appearance of the system status-bar text and icons.</summary>
public enum StatusBarAppearance
{
    /// <summary>Follow platform appearance policy (normally the system light/dark theme), not canvas pixel sampling.</summary>
    Auto = 0,
    /// <summary>Light text and icons, suitable for a dark background.</summary>
    Light = 1,
    /// <summary>Dark text and icons, suitable for a light background.</summary>
    Dark = 2,
}
