namespace Xui.Core.Actual;

/// <summary>
/// Optional, window-scoped vibration playback. Call on the UI thread. Each Play
/// replaces this service's previous playback; stop when the owning interaction ends.
/// Desktop runtimes need not register this service; emulators provide silent playback.
/// </summary>
public interface IVibration
{
    /// <summary>Whether the device can produce vibration.</summary>
    bool IsSupported { get; }

    /// <summary>Whether normalized frequency/texture control is available.</summary>
    bool SupportsFrequency { get; }

    /// <summary>
    /// Starts or updates continuous vibration until Stop. Strength and frequency
    /// must be finite and in [0, 1]. Frequency describes texture, not physical Hz:
    /// iOS maps it to sharpness; Android uses envelope sharpness when supported.
    /// Android may replace the native effect when updating; otherwise frequency is ignored.
    /// Zero strength silences playback. Native settings and interruptions still apply.
    /// </summary>
    void Play(nfloat strength, nfloat frequency);

    /// <summary>
    /// Plays linearly interpolated control points, optionally repeating until Stop.
    /// At least two points are required, beginning at zero with strictly increasing
    /// times, up to 30 seconds. Implementations copy or consume the span before returning.
    /// Native devices may resample points or soften the pattern boundaries.
    /// </summary>
    void Play(scoped ReadOnlySpan<VibrationPoint> points, bool repeat = false);

    /// <summary>Stops playback and cancels pending starts. Safe to call repeatedly.</summary>
    void Stop();
}
