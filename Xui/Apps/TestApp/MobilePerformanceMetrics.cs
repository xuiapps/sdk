using System.Diagnostics;
using Xui.Core.Abstract.Events;

namespace Xui.Apps.TestApp;

/// <summary>
/// Collects rolling frame-rate and managed-memory metrics for the mobile diagnostic overlay,
/// and exposes the window's shared layout traversal counters.
/// </summary>
public sealed class MobilePerformanceMetrics
{
    private const int MaxSamples = 60;
    private static readonly long WindowTicks = Stopwatch.Frequency / 2;
    private readonly long[] timestamps = new long[MaxSamples];
    private int index;
    private int count;
    private long previousAllocation;
    private bool hasPreviousAllocation;
    private long frameStart;
    private readonly LayoutFrameInstruments layoutInstruments;

    /// <summary>Initializes mobile metrics backed by the window's layout instrumentation sink.</summary>
    public MobilePerformanceMetrics(LayoutFrameInstruments layoutInstruments)
    {
        this.layoutInstruments = layoutInstruments;
    }

    public double FramesPerSecond { get; private set; }
    public double DisplayLinkFramesPerSecond { get; private set; }
    public double LastFrameMilliseconds { get; private set; }
    public long AllocatedBytes { get; private set; }
    public long HeapBytes { get; private set; }
    public int SampleCount => count;
    public bool IsEnabled { get; set; }
    public int AnimationCount => layoutInstruments.AnimationCount;
    public int MeasureCount => layoutInstruments.MeasureCount;
    public int ArrangeCount => layoutInstruments.ArrangeCount;
    public int RenderCount => layoutInstruments.RenderCount;

    /// <summary>Records the frame interval requested by the most recent iOS display-link callback.</summary>
    public void RecordDisplayLinkCadence(in FrameEventRef frame)
    {
        var interval = frame.Next - frame.Previous;
        DisplayLinkFramesPerSecond = interval > TimeSpan.Zero
            ? 1d / interval.TotalSeconds
            : 0;
    }

    public void BeginFrame()
    {
        frameStart = Stopwatch.GetTimestamp();
        var allocation = GC.GetAllocatedBytesForCurrentThread();
        AllocatedBytes = hasPreviousAllocation ? Math.Max(0, allocation - previousAllocation) : 0;
        previousAllocation = allocation;
        hasPreviousAllocation = true;
        HeapBytes = GC.GetTotalMemory(false);
    }

    public void EndFrame()
    {
        LastFrameMilliseconds = (Stopwatch.GetTimestamp() - frameStart) * 1000d / Stopwatch.Frequency;
        timestamps[index] = frameStart;
        index = (index + 1) % MaxSamples;
        count = Math.Min(count + 1, MaxSamples);
        if (count < 2)
        {
            FramesPerSecond = 0;
            return;
        }

        var newest = (index - 1 + MaxSamples) % MaxSamples;
        var cutoff = timestamps[newest] - WindowTicks;
        var used = 0;
        for (var offset = 0; offset < count; offset++)
        {
            var sample = (index - 1 - offset + MaxSamples * 2) % MaxSamples;
            if (timestamps[sample] < cutoff)
                break;
            used++;
        }

        var oldest = (index - used + MaxSamples * 2) % MaxSamples;
        var elapsed = (double)(timestamps[newest] - timestamps[oldest]) / Stopwatch.Frequency;
        FramesPerSecond = used > 1 && elapsed > 0 ? (used - 1) / elapsed : 0;
    }
}
