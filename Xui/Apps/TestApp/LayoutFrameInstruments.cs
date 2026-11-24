using System.Diagnostics;
using Xui.Core.Debug;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>
/// Preallocated TestApp instrumentation sink that counts view traversal during one window frame.
/// </summary>
public sealed class LayoutFrameInstruments : IInstruments, IInstrumentsSink, IRenderSurfaceInstrumentsSink
{
    private const int ExpectedViewCount = 4096;

    private readonly HashSet<View>? measuredOrArrangedViews;
    private long frameStartTimestamp;
    private bool frameOpen;

    public int AnimationCount { get; private set; }
    public int MeasureCount { get; private set; }
    public int ArrangeCount { get; private set; }
    public int RenderCount { get; private set; }
    public int TextMeasureCount { get; private set; }
    public int UniqueViewCount => measuredOrArrangedViews?.Count ?? 0;
    public double LastFrameMilliseconds { get; private set; }
    public double EstimatedFramesPerSecond { get; private set; }
    public double TextMeasureMilliseconds => textMeasureTicks * 1000d / Stopwatch.Frequency;

    private long textMeasureTicks;

    /// <summary>Diagnostic name of the render surface whose counters this instance records.</summary>
    public string Name { get; }

    /// <summary>Creates the application-level factory instance.</summary>
    public LayoutFrameInstruments() : this("TestApp") { }

    private LayoutFrameInstruments(string name) : this(name, new HashSet<View>(ExpectedViewCount)) { }

    private LayoutFrameInstruments(string name, HashSet<View>? uniqueViews)
    {
        this.Name = name;
        measuredOrArrangedViews = uniqueViews;
    }

    /// <summary>Creates traversal counters without retaining views for unique-view diagnostics.</summary>
    /// <remarks>Useful for restricted hosts; UniqueViewCount remains zero.</remarks>
    public static LayoutFrameInstruments CreateCountersOnly() => new("TestApp", null);

    /// <summary>Returns this preallocated sink for the TestApp's single UI run loop.</summary>
    public IInstrumentsSink CreateSink() => this;

    /// <summary>Creates isolated counters for one window render surface.</summary>
    public IRenderSurfaceInstrumentsSink CreateRenderSurface(string name) => new LayoutFrameInstruments(name);

    /// <summary>Resets counters at the first animation or render callback for a frame.</summary>
    public void BeginFrame()
    {
        if (frameOpen)
            return;

        frameOpen = true;
        frameStartTimestamp = Stopwatch.GetTimestamp();
        AnimationCount = 0;
        MeasureCount = 0;
        ArrangeCount = 0;
        RenderCount = 0;
        TextMeasureCount = 0;
        textMeasureTicks = 0;
        measuredOrArrangedViews?.Clear();
    }

    /// <summary>Records the CPU time spent in the completed window update.</summary>
    public void EndFrame()
    {
        if (!frameOpen)
            return;

        LastFrameMilliseconds = ElapsedMilliseconds;
        EstimatedFramesPerSecond = LastFrameMilliseconds > 0
            ? 1000d / LastFrameMilliseconds
            : 0;
        frameOpen = false;
    }

    /// <summary>Returns elapsed CPU time so the footer can show the current update before it ends.</summary>
    public double ElapsedMilliseconds => frameOpen
        ? (Stopwatch.GetTimestamp() - frameStartTimestamp) * 1000d / Stopwatch.Frequency
        : LastFrameMilliseconds;

    /// <inheritdoc/>
    public void TrackView(Scope scope, View view)
    {
        switch (scope)
        {
            case Scope.ViewAnimation:
                AnimationCount++;
                break;
            case Scope.ViewMeasure:
                MeasureCount++;
                measuredOrArrangedViews?.Add(view);
                break;
            case Scope.ViewArrange:
                ArrangeCount++;
                measuredOrArrangedViews?.Add(view);
                break;
            case Scope.ViewRendering:
                RenderCount++;
                break;
        }
    }

    /// <inheritdoc/>
    public void TrackTextMeasure(long elapsedTicks)
    {
        TextMeasureCount++;
        textMeasureTicks += elapsedTicks;
    }

    // This sink is intentionally structured-counter-only; it does not format log or trace messages.
    public bool IsEnabled(Scope scope, LevelOfDetail lod) => false;
    public void Log(Scope scope, LevelOfDetail lod, ReadOnlySpan<char> message) { }
    public void BeginTrace(Scope scope, LevelOfDetail lod, ReadOnlySpan<char> message) { }
    public void EndTrace() { }
    public void Dispose() { }
}
