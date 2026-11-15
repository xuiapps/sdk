namespace Xui.DevKit.UI.Design;

/// <summary>Token groups affected by a live design-system update.</summary>
[Flags]
public enum DesignSystemChange
{
    /// <summary>No tokens changed.</summary>
    None = 0,
    /// <summary>Palette or appearance changed; redraw without layout.</summary>
    Colors = 1,
    /// <summary>Font family or metrics changed; measure and arrange again.</summary>
    Typography = 2,
    /// <summary>Spacing or control sizing changed; measure and arrange again.</summary>
    Spacing = 4,
    /// <summary>Corner radii changed; redraw without layout.</summary>
    Shape = 8,
    /// <summary>Animation preferences or timings changed.</summary>
    Motion = 16,
    /// <summary>Initial application or a complete refresh of every token group.</summary>
    All = Colors | Typography | Spacing | Shape | Motion,
}
