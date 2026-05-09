using Xui.Core.Animation;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// An easing curve paired with a default duration for animation.
/// Wraps <see cref="Easing.CubicBezier"/> from Xui.Core.Animation.
/// </summary>
public readonly struct CurveToken
{
    /// <summary>The cubic Bezier easing curve.</summary>
    public Easing.CubicBezier Curve { get; init; }

    /// <summary>Default duration for this easing curve.</summary>
    public TimeSpan DefaultDuration { get; init; }
}
