namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// Selects which color group from the design system a widget should use.
/// </summary>
public enum ColorRole
{
    /// <summary>Brand / primary action colors.</summary>
    Primary,
    /// <summary>Supporting / secondary action colors.</summary>
    Secondary,
    /// <summary>Tertiary highlight / accent colors.</summary>
    Tertiary,
    /// <summary>Warning / caution state colors.</summary>
    Warning,
    /// <summary>Error / destructive state colors.</summary>
    Error,
    /// <summary>Low-contrast neutral colors for quiet buttons, toolbars, etc.</summary>
    Neutral,
}
