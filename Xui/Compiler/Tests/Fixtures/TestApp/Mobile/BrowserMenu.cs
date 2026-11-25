using Xui.Core.UI;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Canvas;
using NFloat = System.Runtime.InteropServices.NFloat;
using Xui.Apps.TestApp.Pages.Canvas.Tests;

namespace Xui.Apps.TestApp;

/// <summary>First browser catalogue: real TestApp cards and navigation, without optional async/GPU showcases.</summary>
public sealed class BrowserMenu : View
{
    public static View CreateRoot(MobilePerformanceMetrics metrics) =>
        new TestAppDesignRoot(new MobileTestAppShell(metrics, new BrowserMenu(), diagnosticsOverlay: null));

    private readonly MobileDemoCategoryPanel canvas;
    public BrowserMenu()
    {
        Margin = (0, 16);
        canvas = new MobileDemoCategoryPanel("Canvas");
        canvas.AddExample<BrowserCanvasExample>("CanvasDrawing", "Canvas Drawing", MobileDemoIcon.Drawing);
        canvas.AddExample<TextMetricsExample>("TextMetrics", "Text Metrics", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserQuadraticCurveExample>("QuadraticCurve", "Quadratic Curve", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserCubicCurveExample>("CubicCurve", "Cubic Curve", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserHeartCurveExample>("HeartCurve", "Heart Curve", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserArcExample>("Arc", "Arcs", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserArcFlowerExample>("ArcFlower", "Arc Flower", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserEllipseExample>("Ellipse", "Ellipses", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserArcToExample>("ArcTo", "Arc To", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserArcToFlowerExample>("ArcToFlower", "Arc To Flower", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserStarExample>("Star", "Star", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserRoundRectExample>("RoundRect", "Rounded Rectangles", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserLineCapJoinExample>("LineCapJoin", "Line Caps and Joins", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserDashPatternExample>("DashPattern", "Dash Patterns", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserFillRuleExample>("FillRule", "Fill Rules", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserPathContinuationExample>("PathContinuation", "Path Continuation", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserClipExample>("Clip", "Clipping", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserTransformExample>("Transform", "Transforms", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserGlobalAlphaExample>("GlobalAlpha", "Transparency", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserTextExample>("Text", "Text", MobileDemoIcon.Drawing);
        canvas.AddExample<BrowserRainbowTextFillExample>("RainbowTextFill", "Rainbow Text", MobileDemoIcon.Drawing);
        AddProtectedChild(canvas);
    }
    public override int Count => 1;
    public override View this[int index] => index == 0 ? canvas : throw new System.IndexOutOfRangeException();
}

public sealed class BrowserCanvasExample : Example
{
    public BrowserCanvasExample() { Title = "Canvas Drawing"; Content = new FillRectTest(); }
}

/// <summary>Fits the unchanged TestApp drawing's artwork bounds inside the example page.</summary>
internal abstract class BrowserDrawingExample<T> : Example where T : View, ICanvasDrawingExample, new()
{
    protected BrowserDrawingExample(string title) { Title = title; Content = new Drawing(); }

    private sealed class Drawing : ViewCollection
    {
        private readonly T artwork = new();
        private readonly TransformView drawing;
        public Drawing() => Add(drawing = new TransformView { Content = artwork });
        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            drawing.Measure((600, 300), context);
            return available;
        }
        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            var bounds = artwork.GetArtworkBounds(context);
            var scale = NFloat.Max(0.001f, NFloat.Min((rect.Width - 32) / bounds.Width, (rect.Height - 32) / bounds.Height));
            drawing.Arrange((0, 0, 600, 300), context);
            drawing.Transform = new AffineTransform(scale, 0, 0, scale,
                rect.Center.X - bounds.Center.X * scale, rect.Center.Y - bounds.Center.Y * scale);
        }
    }
}

internal sealed class BrowserQuadraticCurveExample : BrowserDrawingExample<QuadraticCurveTest>
{
    public BrowserQuadraticCurveExample() : base("Quadratic Curve") { }
}
internal sealed class BrowserCubicCurveExample : BrowserDrawingExample<CubicCurveTest>
{
    public BrowserCubicCurveExample() : base("Cubic Curve") { }
}
internal sealed class BrowserHeartCurveExample : BrowserDrawingExample<HeartCurveTest>
{
    public BrowserHeartCurveExample() : base("Heart Curve") { }
}
internal sealed class BrowserArcExample : BrowserDrawingExample<ArcTest>
{
    public BrowserArcExample() : base("Arcs") { }
}
internal sealed class BrowserArcFlowerExample : BrowserDrawingExample<ArcFlowerTest>
{
    public BrowserArcFlowerExample() : base("Arc Flower") { }
}
internal sealed class BrowserEllipseExample : BrowserDrawingExample<EllipseTest>
{
    public BrowserEllipseExample() : base("Ellipses") { }
}
internal sealed class BrowserArcToExample : BrowserDrawingExample<ArcToTest>
{
    public BrowserArcToExample() : base("Arc To") { }
}
internal sealed class BrowserArcToFlowerExample : BrowserDrawingExample<ArcToFlowerTest>
{
    public BrowserArcToFlowerExample() : base("Arc To Flower") { }
}
internal sealed class BrowserStarExample : BrowserDrawingExample<StarTest>
{
    public BrowserStarExample() : base("Star") { }
}
internal sealed class BrowserRoundRectExample : BrowserDrawingExample<RoundRectTest>
{
    public BrowserRoundRectExample() : base("Rounded Rectangles") { }
}
internal sealed class BrowserLineCapJoinExample : BrowserDrawingExample<LineCapJoinTest>
{
    public BrowserLineCapJoinExample() : base("Line Caps and Joins") { }
}
internal sealed class BrowserDashPatternExample : BrowserDrawingExample<DashPatternTest>
{
    public BrowserDashPatternExample() : base("Dash Patterns") { }
}
internal sealed class BrowserFillRuleExample : BrowserDrawingExample<FillRuleTest>
{
    public BrowserFillRuleExample() : base("Fill Rules") { }
}
internal sealed class BrowserPathContinuationExample : BrowserDrawingExample<PathContinuationTest>
{
    public BrowserPathContinuationExample() : base("Path Continuation") { }
}
internal sealed class BrowserClipExample : BrowserDrawingExample<ClipTest>
{
    public BrowserClipExample() : base("Clipping") { }
}
internal sealed class BrowserTransformExample : BrowserDrawingExample<TransformTest>
{
    public BrowserTransformExample() : base("Transforms") { }
}
internal sealed class BrowserGlobalAlphaExample : BrowserDrawingExample<GlobalAlphaTest>
{
    public BrowserGlobalAlphaExample() : base("Transparency") { }
}
internal sealed class BrowserTextExample : BrowserDrawingExample<TextTest>
{
    public BrowserTextExample() : base("Text") { }
}
internal sealed class BrowserRainbowTextFillExample : BrowserDrawingExample<RainbowTextFillTest>
{
    public BrowserRainbowTextFillExample() : base("Rainbow Text") { }
}
