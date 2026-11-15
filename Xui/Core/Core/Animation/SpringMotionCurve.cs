namespace Xui.Core.Animation;

/// <summary>A critically damped spring evaluated analytically, independent of display refresh rate.</summary>
public readonly struct SpringMotionCurve
{
    private static readonly nfloat Frequency = 18;
    private readonly nfloat displacement;
    private readonly nfloat coefficient;

    /// <summary>The time at which the rebound starts.</summary>
    public TimeSpan StartTime { get; }
    /// <summary>The exact resting position.</summary>
    public nfloat Target { get; }

    /// <summary>Creates a spring preserving initial position and velocity.</summary>
    public SpringMotionCurve(TimeSpan startTime, nfloat position, nfloat velocity, nfloat target)
    {
        StartTime = startTime;
        Target = target;
        displacement = position - target;
        coefficient = velocity + Frequency * displacement;
    }

    /// <summary>Evaluates the position at an absolute frame time.</summary>
    public nfloat this[TimeSpan time]
    {
        get
        {
            var t = nfloat.Max(0, (nfloat)(time - StartTime).TotalSeconds);
            return Target + (displacement + coefficient * t) * nfloat.Exp(-Frequency * t);
        }
    }

    /// <summary>Evaluates the velocity in units per second.</summary>
    public nfloat VelocityAt(TimeSpan time)
    {
        var t = nfloat.Max(0, (nfloat)(time - StartTime).TotalSeconds);
        return (coefficient - Frequency * (displacement + coefficient * t)) * nfloat.Exp(-Frequency * t);
    }

    /// <summary>Whether the curve is close enough to finish at its exact target.</summary>
    public bool IsComplete(TimeSpan time) => time >= StartTime &&
        ((time - StartTime).TotalSeconds >= 0.8 ||
         (nfloat.Abs(this[time] - Target) < 0.1f && nfloat.Abs(VelocityAt(time)) < 1));
}
