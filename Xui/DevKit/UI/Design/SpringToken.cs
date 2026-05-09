namespace Xui.DevKit.UI.Design;

/// <summary>
/// Spring physics parameters for animation.
/// </summary>
public readonly struct SpringToken
{
    /// <summary>Spring stiffness (higher = faster oscillation).</summary>
    public float Stiffness { get; init; }

    /// <summary>Damping ratio (1.0 = critically damped, less than 1.0 = bouncy).</summary>
    public float Damping { get; init; }
}
