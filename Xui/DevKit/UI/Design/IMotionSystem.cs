namespace Xui.DevKit.UI.Design;

/// <summary>
/// Provides motion tokens for animations.
/// </summary>
public interface IMotionSystem
{
    /// <summary>Whether springs or curves are preferred for interactive feedback.</summary>
    MotionPreference Preference { get; }

    /// <summary>True if the user has requested reduced motion (accessibility).</summary>
    bool ReducedMotion { get; }

    /// <summary>Elements entering the screen — (0.05, 0.7, 0.1, 1.0), 400 ms.</summary>
    CurveToken EmphasizedDecelerate { get; }

    /// <summary>Elements leaving the screen — (0.3, 0.0, 0.8, 0.15), 200 ms.</summary>
    CurveToken EmphasizedAccelerate { get; }

    /// <summary>General transitions — (0.2, 0.0, 0.0, 1.0), 300 ms.</summary>
    CurveToken Standard { get; }

    /// <summary>Settling transitions — (0.0, 0.0, 0.0, 1.0), 250 ms.</summary>
    CurveToken StandardDecelerate { get; }

    /// <summary>Quick dismissals — (0.3, 0.0, 1.0, 1.0), 200 ms.</summary>
    CurveToken StandardAccelerate { get; }

    /// <summary>Progress bars, continuous — (0.0, 0.0, 1.0, 1.0), any duration.</summary>
    CurveToken Linear { get; }

    /// <summary>Art / lifestyle apps, button press — stiffness 600, damping 0.5.</summary>
    SpringToken SpringBouncy { get; }

    /// <summary>Default interactive feedback — stiffness 300, damping 0.8.</summary>
    SpringToken SpringResponsive { get; }

    /// <summary>Business / utility apps, modals — stiffness 200, damping 1.0.</summary>
    SpringToken SpringSmooth { get; }
}
