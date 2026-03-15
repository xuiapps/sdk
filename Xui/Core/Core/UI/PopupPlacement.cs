namespace Xui.Core.UI;

/// <summary>
/// Specifies the preferred placement of a popup relative to its anchor rect.
/// The platform may adjust placement to keep the popup on screen.
/// </summary>
public enum PopupPlacement
{
    /// <summary>Show below the anchor (default for dropdowns/comboboxes).</summary>
    Below,

    /// <summary>Show above the anchor.</summary>
    Above,

    /// <summary>Show to the right of the anchor (e.g. nested/submenu).</summary>
    Right,

    /// <summary>Show to the left of the anchor.</summary>
    Left,
}
