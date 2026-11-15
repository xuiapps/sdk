namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>A backdrop material rather than a filter of the group's foreground drawing.</summary>
public enum BackdropMaterial
{
    /// <summary>No backdrop material.</summary>
    None,
    /// <summary>Native system material blur where available.</summary>
    Blur,
    /// <summary>Native Liquid Glass on supported iOS versions; system blur on older iOS.</summary>
    LiquidGlass
}
