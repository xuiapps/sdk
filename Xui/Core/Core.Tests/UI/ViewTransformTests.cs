using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Actual;

namespace Xui.Core.UI.Tests;

public class ViewTransformTests
{
    [Fact]
    public void CoordinateConversions_AreIdentityByDefault()
    {
        var view = new View();
        var point = new Point(12, 34);

        Assert.Equal(point, view.TransformPoint(point));
        Assert.Equal(point, view.InverseTransformPoint(point));
        Assert.Equal(point, view.LocalToGlobal(point));
        Assert.Equal(point, view.GlobalToLocal(point));
    }

    [Fact]
    public void CoordinateConversions_ComposeEachAncestorInTheCorrectDirection()
    {
        var root = new ViewCollection();
        var parent = new OffsetView((10, 20));
        var child = new OffsetView((3, 4));
        root.Add(parent);
        parent.Add(child);

        Assert.Equal(new Point(15, 26), child.LocalToGlobal((2, 2)));
        Assert.Equal(new Point(2, 2), child.GlobalToLocal((15, 26)));
    }

    [Fact]
    public void TransformView_MapsInputAndCullingThroughItsPresentationTransform()
    {
        var content = new FrameProbeView { Size = (100, 80) };
        var host = new TransformView
        {
            Content = content,
            Transform = AffineTransform.Translate((30, 40)),
        };
        host.Arrange(new Rect(0, 0, 100, 80), null!);

        Assert.Equal(new Point(5, 6), host.TransformPoint((35, 46)));
        Assert.Equal(new Point(35, 46), host.InverseTransformPoint((5, 6)));
        Assert.Equal(new Rect(5, 6, 10, 12), host.TransformRect((35, 46, 10, 12)));
        Assert.Equal(new Rect(30, 40, 100, 80), host.CullingFrame);
    }

    [Fact]
    public void ScrollView_MapsContentInputThroughItsScrollOffsetWithoutRearrangingContent()
    {
        var content = new FrameProbeView { Size = (300, 800) };
        var scrollView = new ScrollView { Content = content };
        scrollView.Arrange(new Rect(100, 200, 300, 400), null!);

        Assert.Equal(new Rect(0, 0, 300, 800), content.ArrangedFrame);

        var wheel = new Xui.Core.Abstract.Events.ScrollWheelEventRef { Delta = (0, -120) };
        scrollView.OnScrollWheel(ref wheel);

        Assert.Equal(new Point(20, 40), scrollView.GlobalToLocal((120, 240)));
        Assert.Equal(new Point(20, 120), content.GlobalToLocal((120, 240)));
        Assert.Equal(new Point(120, 240), content.LocalToGlobal((20, 120)));
        Assert.Equal(new Rect(0, 0, 300, 800), content.ArrangedFrame);
    }

    [Fact]
    public void VerticalScrollView_StretchesNarrowContentToTheViewportWidth()
    {
        var content = new FrameProbeView { Size = (80, 800) };
        var scrollView = new ScrollView { Content = content };

        scrollView.Arrange(new Rect(100, 200, 300, 400), null!);

        Assert.Equal(new Rect(0, 0, 300, 800), content.ArrangedFrame);
    }

    [Fact]
    public void ScrollView_MapsCullingFramesThroughItsFrameAndScrollOffset()
    {
        var content = new FrameProbeView { Size = (300, 800) };
        var scrollView = new ScrollView { Content = content };
        scrollView.Arrange(new Rect(100, 200, 300, 400), null!);

        var wheel = new Xui.Core.Abstract.Events.ScrollWheelEventRef { Delta = (0, -120) };
        scrollView.OnScrollWheel(ref wheel);

        Assert.Equal(new Rect(20, 40, 50, 60), scrollView.TransformRect((120, 240, 50, 60)));
        Assert.Equal(new Rect(120, 240, 50, 60), scrollView.InverseTransformRect((20, 40, 50, 60)));
    }

    [Fact]
    public void Render_SkipsChildrenWhoseCullingFrameIsOutsideTheWindow()
    {
        var visible = new CountingRenderView();
        var offscreen = new CountingRenderView();
        var root = new CullingStack();
        root.Add(visible);
        root.Add(offscreen);
        root.Arrange(new Rect(0, 0, 100, 100), null!);

        using var context = new SvgDrawingContext((100, 100), Stream.Null);
        root.Render(context);

        Assert.Equal(1, visible.RenderCount);
        Assert.Equal(0, offscreen.RenderCount);
    }

    [Fact]
    public void ScrollView_ReplacingContentDetachesThePreviousContent()
    {
        var first = new View();
        var second = new View();
        var scrollView = new ScrollView { Content = first };

        scrollView.Content = second;

        Assert.Null(first.Parent);
        Assert.Same(second, scrollView.Content);
        Assert.NotNull(second.Parent);
    }

    private sealed class OffsetView(Vector offset) : ViewCollection
    {
        public override Point TransformPoint(Point point) => point - offset;

        public override Point InverseTransformPoint(Point point) => point + offset;
    }

    private sealed class FrameProbeView : FixedView
    {
        public Rect ArrangedFrame => this.Frame;
    }

    private sealed class CullingStack : ViewCollection
    {
        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            this[0].Arrange(new Rect(rect.X, rect.Y, rect.Width, 90), context);
            this[1].Arrange(new Rect(rect.X, rect.Y + 200, rect.Width, 90), context);
        }
    }

    private sealed class CountingRenderView : View
    {
        public int RenderCount { get; private set; }

        protected override void RenderCore(IContext context) => this.RenderCount++;
    }
}
