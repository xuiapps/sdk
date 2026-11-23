using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

/// <summary>Bring the requested layer in over 300 ms, then remove the old layer over 300 ms.</summary>
public sealed class FloorplanLayerTransition
{
    public int Current { get; private set; }
    public int Incoming { get; private set; }
    public int Requested { get; private set; }
    public bool Active => Current != Incoming;
    public NFloat CurrentOpacity => !Active || elapsed <= Duration ? 1 : 1 - (elapsed - Duration) / Duration;
    public NFloat IncomingOpacity => !Active ? 0 : NFloat.Min(1, elapsed / Duration);
    private NFloat elapsed;
    private static readonly NFloat Duration = (NFloat).3;

    public void Request(int level)
    {
        if ((uint)level > 3) throw new ArgumentOutOfRangeException(nameof(level));
        Requested = level;
        if (!Active && level != Current) { Incoming = level; elapsed = 0; }
    }

    public bool Advance(NFloat seconds)
    {
        if (!Active || seconds <= 0) return false;
        elapsed = NFloat.Min(Duration * 2, elapsed + seconds);
        if (elapsed >= Duration * 2)
        {
            Current = Incoming;
            elapsed = 0;
            Request(Requested); // Rapid gestures queue only the latest requested detail.
        }
        return true;
    }
}
