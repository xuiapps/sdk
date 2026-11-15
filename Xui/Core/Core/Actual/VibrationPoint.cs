namespace Xui.Core.Actual;

/// <summary>A vibration's normalized strength and frequency/texture at an absolute pattern time.</summary>
/// <param name="Time">Time from the beginning of the pattern.</param>
/// <param name="Strength">Strength in [0, 1].</param>
/// <param name="Frequency">Texture in [0, 1], from soft/low to crisp/high; not Hertz.</param>
public readonly record struct VibrationPoint(TimeSpan Time, nfloat Strength, nfloat Frequency);
