using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xui.Core.Actual;
using Xui.Runtime.IOS.CoreHaptics;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Serializes native work off the UI thread and coalesces live control updates.</summary>
internal sealed class IOSVibration : IVibration, IDisposable
{
    private readonly object gate = new();
    private HapticSession? session;
    private VibrationPoint[]? pattern;
    private NFloat strength, frequency;
    private bool repeat, requested, disposed, working;
    private long version;
    public bool IsSupported { get; } = HapticSession.IsSupported;
    public bool SupportsFrequency => IsSupported;

    public void Play(NFloat strength, NFloat frequency)
    {
        VibrationPattern.Validate(strength, frequency);
        lock (gate)
        {
            if (disposed) return;
            this.strength = strength; this.frequency = frequency; pattern = null;
            requested = strength > 0;
            Queue();
        }
    }

    public void Play(scoped ReadOnlySpan<VibrationPoint> points, bool repeat = false)
    {
        VibrationPattern.Validate(points);
        lock (gate)
        {
            if (disposed) return;
            pattern = points.ToArray(); this.repeat = repeat; requested = true;
            Queue();
        }
    }

    public void Stop()
    {
        lock (gate) { if (disposed) return; requested = false; pattern = null; Queue(); }
    }

    private void Queue()
    {
        version++;
        if (working || !IsSupported) return;
        working = true;
        _ = Task.Run(Process);
    }

    private void Process()
    {
        while (true)
        {
            long command;
            bool play, loop, close;
            NFloat s, f;
            VibrationPoint[]? points;
            lock (gate)
            {
                command = version; play = requested; loop = repeat; close = disposed;
                s = strength; f = frequency; points = pattern;
            }
            try
            {
                if (close || !play) { session?.Dispose(); session = null; }
                else
                {
                    // Starting the engine can block. Do it here, never in input dispatch.
                    session ??= new HapticSession();
                    lock (gate)
                    {
                        // A stop/navigation during startup must not produce a late pulse.
                        if (command != version) continue;
                        if (points is null) session.Control(s, f);
                        else session.Play(points, loop);
                    }
                }
            }
            catch (Exception error)
            {
                Trace.WriteLine(error);
                session?.Dispose(); session = null;
                // Do not restart an interrupted pattern by itself. A new explicit Play
                // recreates players and the engine after a native reset/interruption.
            }
            lock (gate)
            {
                if (command != version) continue;
                working = false;
                return;
            }
        }
    }

    public void Dispose()
    {
        lock (gate) { if (disposed) return; disposed = true; requested = false; pattern = null; Queue(); }
    }
}
