using Xui.Core.Animation;
using Xui.Core.DI;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// Concrete motion system providing both curve-based and spring-based animation tokens.
/// Durations are scaled by <see cref="MotionPreset"/>.
/// </summary>
internal class XuiMotionSystem : IMotionSystem
{
    private readonly nfloat durationScale;

    public XuiMotionSystem(XuiDesignSystemOptions options, IDeviceInfo device)
    {
        Preference = options.MotionPreference;
        ReducedMotion = device.PrefersReducedMotion || options.Motion == MotionPreset.None;

        durationScale = options.Motion switch
        {
            MotionPreset.None  => 0,
            MotionPreset.Short => 0.5f,
            MotionPreset.Normal => 1.0f,
            MotionPreset.Long  => 2.0f,
            _ => 1.0f,
        };
    }

    private TimeSpan Scale(double baseMs) =>
        TimeSpan.FromMilliseconds(baseMs * (double)durationScale);

    /// <inheritdoc/>
    public MotionPreference Preference { get; }

    /// <inheritdoc/>
    public bool ReducedMotion { get; }

    /// <inheritdoc/>
    public CurveToken EmphasizedDecelerate => new() { Curve = new Easing.CubicBezier((0.05f, 0.7f), (0.1f, 1.0f)), DefaultDuration = Scale(400) };

    /// <inheritdoc/>
    public CurveToken EmphasizedAccelerate => new() { Curve = new Easing.CubicBezier((0.3f, 0.0f), (0.8f, 0.15f)), DefaultDuration = Scale(200) };

    /// <inheritdoc/>
    public CurveToken Standard => new() { Curve = new Easing.CubicBezier((0.2f, 0.0f), (0.0f, 1.0f)), DefaultDuration = Scale(300) };

    /// <inheritdoc/>
    public CurveToken StandardDecelerate => new() { Curve = new Easing.CubicBezier((0.0f, 0.0f), (0.0f, 1.0f)), DefaultDuration = Scale(250) };

    /// <inheritdoc/>
    public CurveToken StandardAccelerate => new() { Curve = new Easing.CubicBezier((0.3f, 0.0f), (1.0f, 1.0f)), DefaultDuration = Scale(200) };

    /// <inheritdoc/>
    public CurveToken Linear => new() { Curve = new Easing.CubicBezier((0.0f, 0.0f), (1.0f, 1.0f)), DefaultDuration = Scale(300) };

    /// <inheritdoc/>
    public SpringToken SpringBouncy => new() { Stiffness = 600, Damping = 0.5f };

    /// <inheritdoc/>
    public SpringToken SpringResponsive => new() { Stiffness = 300, Damping = 0.8f };

    /// <inheritdoc/>
    public SpringToken SpringSmooth => new() { Stiffness = 200, Damping = 1.0f };
}
