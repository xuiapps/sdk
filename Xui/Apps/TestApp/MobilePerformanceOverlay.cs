using System.Globalization;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>Small navigation-owned diagnostics surface for live mobile frame and memory metrics.</summary>
internal sealed class MobilePerformanceOverlay : View
{
    private static readonly Size OverlaySize = (248, 66);
    private static readonly NumberFormatInfo NumberFormat = new() { NumberDecimalSeparator = "," };
    private readonly MobilePerformanceMetrics metrics;
    private bool isEnabled;

    public MobilePerformanceOverlay(MobilePerformanceMetrics metrics)
    {
        this.metrics = metrics;
        Margin = (50, 0, 0, 0);
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value)
                return;
            isEnabled = value;
            InvalidateRender();
            if (isEnabled)
                RequestAnimationFrame();
        }
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) => OverlaySize;

    // Diagnostics must never block the form beneath them, even when hidden.
    public override bool HitTest(Point point) => false;

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (isEnabled)
        {
            InvalidateRender();
            RequestAnimationFrame();
        }
    }

    protected override void RenderCore(IContext context)
    {
        if (!isEnabled)
            return;

        var frame = Frame;
        context.BeginPath();
        context.RoundRect(frame, 12);
        context.SetFill(Colors.Black);
        context.Fill();

        context.SetFont(new Font(11, "Inter", FontWeight.Medium));
        context.TextAlign = TextAlign.Left;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(Colors.White);
        var x = frame.X + 12;
        var y = frame.Y + 17;
        DrawValue(context, "FPS", metrics.SampleCount < 2 ? "--" :
            ((int)Math.Round(metrics.FramesPerSecond)).ToString(NumberFormat), (x, y), 52);
        DrawValue(context, "Frame", metrics.LastFrameMilliseconds.ToString("F1", NumberFormat), (x + 112, y), 86);
        context.FillText("ms", (x + 204, y));

        // Equal-width cells reserve room for three digits without shifting labels.
        y += 16;
        DrawValue(context, "Anim", metrics.AnimationCount.ToString(NumberFormat), (x, y), 52);
        DrawValue(context, "Msr", metrics.MeasureCount.ToString(NumberFormat), (x + 56, y), 52);
        DrawValue(context, "Arr", metrics.ArrangeCount.ToString(NumberFormat), (x + 112, y), 52);
        DrawValue(context, "Rnd", metrics.RenderCount.ToString(NumberFormat), (x + 168, y), 52);

        y += 16;
        DrawBytes(context, "Alloc", metrics.AllocatedBytes, (x, y), kilobyteThreshold: 10_001);
        DrawBytes(context, "Heap", metrics.HeapBytes, (x + 112, y));
    }

    private static void DrawValue(IContext context, string label, string value, Point position, NFloat valueRight)
    {
        context.TextAlign = TextAlign.Left;
        context.FillText(label, position);
        context.TextAlign = TextAlign.Right;
        context.FillText(value, (position.X + valueRight, position.Y));
        context.TextAlign = TextAlign.Left;
    }

    private static void DrawBytes(IContext context, string label, long value, Point position, long kilobyteThreshold = 1_024)
    {
        string number;
        string unit;
        if (value >= 1_048_576)
        {
            number = (value / 1_048_576.0).ToString("F1", NumberFormat);
            unit = "MB";
        }
        else if (value >= kilobyteThreshold)
        {
            number = (value / 1_024.0).ToString("F1", NumberFormat);
            unit = "KB";
        }
        else
        {
            number = value.ToString(NumberFormat);
            unit = "B";
        }

        DrawValue(context, label, number, position, 76);
        context.FillText(unit, (position.X + 82, position.Y));
    }
}
