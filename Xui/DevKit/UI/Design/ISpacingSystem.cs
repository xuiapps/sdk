namespace Xui.DevKit.UI.Design;

/// <summary>
/// Provides two parallel spacing scales:
/// <list type="bullet">
/// <item><see cref="Passive"/> — for layout, content gaps, margins, card padding.</item>
/// <item><see cref="Active"/> — for interactive leaf elements (button padding, input fields, hit targets).
/// On desktop, Active aligns with Passive (tight). On touch, Active shifts up
/// so that buttons/inputs get generous padding while layout stays compact.</item>
/// </list>
/// </summary>
public interface ISpacingSystem
{
    /// <summary>Layout/content spacing scale.</summary>
    SpacingScale Passive { get; }

    /// <summary>Interactive element spacing scale (shifted up for touch targets).</summary>
    SpacingScale Active { get; }

    /// <summary>0 pt.</summary>
    nfloat None { get; }
}
