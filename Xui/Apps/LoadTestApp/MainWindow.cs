using System.Diagnostics;
using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Window = Xui.Core.Abstract.Window;

namespace Xui.Apps.LoadTestApp;

public class MainWindow : Window
{
    public MainWindow(IServiceProvider context) : base(context)
    {
        this.Title = "Xui LoadTestApp";
    }

    private NFloat CellWidth = 80;
    private NFloat CellHeight = 22;
    private NFloat ScrollY = 0;

    private long previousFrameMemory;
    private bool hasPreviousFrameMemory;

    // --- Perf sampling (fixed circular buffer of 300 samples) ---
    private const int MaxSamples = 300;

    private struct PerfSample
    {
        public long Timestamp;   // Stopwatch ticks
        public long AllocBytes;  // per-frame allocated bytes (diff vs previous frame)
    }

    private readonly PerfSample[] samples = new PerfSample[MaxSamples];
    private int sampleIndex = 0;
    private int sampleCount = 0;

    private static double TicksToSeconds(long ticks)
        => (double)ticks / Stopwatch.Frequency;

    private void AddSample(long allocBytes)
    {
        this.samples[this.sampleIndex] = new PerfSample
        {
            Timestamp = Stopwatch.GetTimestamp(),
            AllocBytes = allocBytes
        };

        this.sampleIndex = (this.sampleIndex + 1) % MaxSamples;
        if (this.sampleCount < MaxSamples)
            this.sampleCount++;
    }

    private void ComputeStats(out double fpsAvg, out long allocMin, out long allocMax, out long allocAvg, out int count)
    {
        count = this.sampleCount;

        if (count < 2)
        {
            fpsAvg = 0;
            allocMin = allocMax = allocAvg = 0;
            return;
        }

        int oldest = (this.sampleIndex - count + MaxSamples) % MaxSamples;
        long firstTs = this.samples[oldest].Timestamp;
        int newest = (this.sampleIndex - 1 + MaxSamples) % MaxSamples;
        long lastTs = this.samples[newest].Timestamp;

        long min = long.MaxValue;
        long max = long.MinValue;
        long sum = 0;

        for (int i = 0; i < count; i++)
        {
            long a = this.samples[(oldest + i) % MaxSamples].AllocBytes;
            if (a < min) min = a;
            if (a > max) max = a;
            sum += a;
        }

        double dt = TicksToSeconds(lastTs - firstTs);
        fpsAvg = dt > 0 ? (count - 1) / dt : 0;

        allocMin = min == long.MaxValue ? 0 : min;
        allocMax = max == long.MinValue ? 0 : max;
        allocAvg = sum / count;
    }

    public override void Render(ref RenderEventRef render)
    {
        // Capture once per frame and diff against previous frame.
        // (This avoids calling GetAllocatedBytesForCurrentThread twice.)
        long memNow = GC.GetAllocatedBytesForCurrentThread();
        long allocated = 0;

        if (this.hasPreviousFrameMemory)
        {
            allocated = memNow - this.previousFrameMemory;

            // If anything weird happens (thread switch, counter reset),
            // clamp to 0 so stats don't explode.
            if (allocated < 0)
                allocated = 0;
        }

        this.previousFrameMemory = memNow;
        this.hasPreviousFrameMemory = true;

        using var ctx = Xui.Core.Actual.Runtime.DrawingContext!;

        // Clear background
        ctx.SetFill(Colors.Black);
        ctx.FillRect(render.Rect);

        // Infinite vertical scroll
        this.ScrollY += 2;

        int startRow = (int)Math.Floor((double)(this.ScrollY / this.CellHeight));
        int endRow = (int)Math.Ceiling((double)((this.ScrollY + render.Rect.Height) / this.CellHeight));
        int startCol = 0;
        int endCol = (int)Math.Ceiling((double)(render.Rect.Width / this.CellWidth));

        ctx.SetFont(new Font
        {
            FontFamily = ["Inter", "sans-serif"],
            FontSize = 11,
            FontWeight = 400
        });
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Middle;

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                NFloat x = col * this.CellWidth;
                NFloat y = row * this.CellHeight - this.ScrollY;

                NFloat r = (NFloat)(0.5 + 0.5 * Math.Sin(col * 0.15 + row * 0.02));
                NFloat g = (NFloat)(0.5 + 0.5 * Math.Sin(col * 0.1 + row * 0.03 + 2));
                NFloat b = (NFloat)(0.5 + 0.5 * Math.Sin(col * 0.12 + row * 0.025 + 4));

                var cellRect = new Rect(x, y, this.CellWidth - 1, this.CellHeight - 1);

                ctx.SetFill(new Color(r, g, b, 1));
                ctx.FillRect(cellRect);

                NFloat brightness = (r + g + b) / 3;
                ctx.SetFill(brightness > 0.5 ? Colors.Black : Colors.White);

                ctx.FillText($"C{col},{row}", (x + this.CellWidth / 2, y + this.CellHeight / 2));
            }
        }

        // --- Perf ---
        this.AddSample(allocated);

        this.ComputeStats(
            out double fpsAvg,
            out long allocMin,
            out long allocMax,
            out long allocAvg,
            out int sampleCount);

        long allocRange = allocMax - allocMin;

        // Overlay (one metric per line, stable-ish formatting)
        ctx.SetFont(new Font
        {
            FontFamily = ["Inter", "sans-serif"],
            FontSize = 14,
            FontWeight = 700
        });
        ctx.TextAlign = TextAlign.Left;
        ctx.TextBaseline = TextBaseline.Top;

        int fpsRounded = (int)Math.Round(fpsAvg);

        static long RoundTo(long value, long step)
            => step <= 1 ? value : ((value + (step / 2)) / step) * step;

        long allocNowR   = RoundTo(allocated, 10);
        long allocAvgR   = RoundTo(allocAvg, 10);
        long allocMinR   = RoundTo(allocMin, 10);
        long allocMaxR   = RoundTo(allocMax, 10);
        long allocRangeR = RoundTo(allocRange, 10);

        const int lines = 4;
        const int lineH = 18;
        int overlayH = 8 + lines * lineH;

        ctx.SetFill(new Color(0, 0, 0, 0.7f));
        ctx.FillRect(new Rect(8, 8, 160, overlayH));

        ctx.SetFill(Colors.Lime);

        int y2 = 12;
        ctx.FillText($"FPS(avg {sampleCount}): {fpsRounded}", (12, y2));
        y2 += lineH;
        ctx.FillText($"Samples: {sampleCount}", (12, y2));
        y2 += lineH;
        ctx.FillText($"Alloc Now: {allocNowR} B", (12, y2));
        y2 += lineH;
        ctx.FillText($"Alloc Range: {allocRangeR}", (12, y2));

        Invalidate();

        base.Render(ref render);
    }
}
