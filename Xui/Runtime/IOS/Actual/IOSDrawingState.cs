using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Managed mirrors of native state needed when changing dash phase or drawing images.</summary>
internal sealed class IOSDrawingState
{
    internal struct Snapshot
    {
        internal NFloat Alpha;
        internal NFloat DashOffset;
        internal int DashStart;
        internal int DashCount;
    }

    private Snapshot current = new() { Alpha = 1 };
    private NFloat[] dashes = [];

    internal NFloat GlobalAlpha { get => current.Alpha; set => current.Alpha = value; }
    internal NFloat LineDashOffset { get => current.DashOffset; set => current.DashOffset = value; }
    internal ReadOnlySpan<NFloat> LineDash => dashes.AsSpan(current.DashStart, current.DashCount);

    internal Snapshot Save() => current;
    internal void Restore(Snapshot state) => current = state;
    internal void Reset() => current = new() { Alpha = 1 };

    internal void SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        // Append so saved states keep their original pattern. Restore rewinds the arena;
        // the next Bind resets it, retaining capacity for allocation-free warm replay.
        int start = checked(current.DashStart + current.DashCount);
        int count = checked(segments.Length * (segments.Length % 2 == 0 ? 1 : 2));
        int required = checked(start + count);
        if (required > dashes.Length)
            Array.Resize(ref dashes, Math.Max(required, Math.Max(16, dashes.Length * 2)));
        segments.CopyTo(dashes.AsSpan(start));
        if (count != segments.Length)
            segments.CopyTo(dashes.AsSpan(start + segments.Length));
        current.DashStart = start;
        current.DashCount = count;
    }

    internal NFloat ImageAlpha(NFloat opacity) =>
        NFloat.Clamp(current.Alpha, 0, 1) * NFloat.Clamp(opacity, 0, 1);
}
