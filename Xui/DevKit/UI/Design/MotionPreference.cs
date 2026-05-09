namespace Xui.DevKit.UI.Design;

/// <summary>
/// Whether the application prefers curve-based (Bezier) or spring-based (physics) animations.
/// </summary>
public enum MotionPreference
{
    /// <summary>Duration + cubic Bezier easing.</summary>
    Curve,

    /// <summary>Spring physics (stiffness + damping).</summary>
    Spring,
}
