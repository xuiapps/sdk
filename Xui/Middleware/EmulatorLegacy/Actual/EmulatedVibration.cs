using System;
using System.Runtime.InteropServices;
using Xui.Core.Actual;

namespace Xui.Middleware.EmulatorLegacy.Actual;

internal sealed class EmulatedVibration : IVibration
{
    public bool IsSupported => false;
    public bool SupportsFrequency => false;
    public void Play(NFloat strength, NFloat frequency) { }
    public void Play(scoped ReadOnlySpan<VibrationPoint> points, bool repeat = false) { }
    public void Stop() { }
}
