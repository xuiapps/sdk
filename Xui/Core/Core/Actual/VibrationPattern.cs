namespace Xui.Core.Actual;

/// <summary>Shared validation and interpolation for native vibration backends.</summary>
public static class VibrationPattern
{
    /// <summary>The portable pattern duration limit, matching one iOS continuous event.</summary>
    public static readonly TimeSpan MaximumDuration = TimeSpan.FromSeconds(30);

    /// <summary>Validates a normalized strength/frequency pair.</summary>
    public static void Validate(nfloat strength, nfloat frequency)
    {
        if (!nfloat.IsFinite(strength) || strength < 0 || strength > 1)
            throw new ArgumentOutOfRangeException(nameof(strength));
        if (!nfloat.IsFinite(frequency) || frequency < 0 || frequency > 1)
            throw new ArgumentOutOfRangeException(nameof(frequency));
    }

    /// <summary>Validates the point count, times, duration and normalized values.</summary>
    public static void Validate(scoped ReadOnlySpan<VibrationPoint> points)
    {
        if (points.Length < 2 || points[0].Time != TimeSpan.Zero)
            throw new ArgumentException("A pattern needs at least two points, starting at time zero.", nameof(points));
        for (var i = 0; i < points.Length; i++)
        {
            Validate(points[i].Strength, points[i].Frequency);
            if (i > 0 && points[i].Time <= points[i - 1].Time)
                throw new ArgumentException("Pattern times must increase strictly.", nameof(points));
        }
        if (points[^1].Time > MaximumDuration)
            throw new ArgumentException("A pattern may be at most 30 seconds long.", nameof(points));
    }

    /// <summary>Samples a validated pattern, clamping outside its time range.</summary>
    public static VibrationPoint Sample(scoped ReadOnlySpan<VibrationPoint> points, TimeSpan time)
    {
        if (time <= points[0].Time) return points[0];
        for (var i = 1; i < points.Length; i++)
        {
            if (time > points[i].Time) continue;
            var a = points[i - 1]; var b = points[i];
            var t = (nfloat)(time - a.Time).TotalSeconds / (nfloat)(b.Time - a.Time).TotalSeconds;
            return new(time, a.Strength + (b.Strength - a.Strength) * t,
                a.Frequency + (b.Frequency - a.Frequency) * t);
        }
        return points[^1];
    }
}
