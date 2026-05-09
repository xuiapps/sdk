namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// Controls the visual weight of a button.
/// </summary>
public enum ButtonVariant
{
    /// <summary>Filled background — highest visual weight. Default for primary actions.</summary>
    Filled,

    /// <summary>Outline border only — medium visual weight. For secondary/grouped actions.</summary>
    Outline,

    /// <summary>Text only — lowest visual weight. Background appears on hover/press only.</summary>
    Text,
}
