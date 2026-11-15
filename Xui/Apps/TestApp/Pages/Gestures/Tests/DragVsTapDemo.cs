using Xui.Core.Canvas;
using Xui.Core.Animation;
using Xui.Apps.TestApp.Pages.Gestures.Widgets;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures.Tests;

/// <summary>Real tap controls and firm drawing gestures negotiate with their scrolling parent.</summary>
public class DragVsTapDemo : ViewCollection
{
    private readonly ScrollView scroller;
    private struct TapBubble
    {
        public bool Visible;
        public Point Origin;
        public NFloat Age;
    }
    private readonly TapBubble[] tapBubbles = new TapBubble[8];
    private int nextBubble;
    public DragVsTapDemo()
    {
        var stack = new VerticalStack { Margin = InputDemoCard.ContentInset };
        var status = Text("Your ideas, saved with a tap.", 12, color: Muted);
        status.Id = "SketchStatus"; status.Margin = (0, 0, 12, 0); stack.Add(status);
        for (var i = 0; i < 3; i++)
        {
            var number = i + 1;
            var button = new Button { Id = $"SaveSketch{number}", Text = $"Save idea {number}", Margin = (0, 0, 10, 0) };
            button.Clicked = () =>
            {
                status.Text = $"Idea {number} saved. Keep creating.";
                var origin = GlobalToLocal(button.Parent!.LocalToGlobal(button.Frame.Center));
                tapBubbles[nextBubble] = new TapBubble { Visible = true, Origin = origin, Age = -1 };
                nextBubble = (nextBubble + 1) % tapBubbles.Length;
                InvalidateRender(); RequestAnimationFrame();
            };
            stack.Add(button);
            stack.Add(new ScratchPad { Id = $"SketchPad{number}", MinimumHeight = 156, MaximumHeight = 156, Margin = (0, 0, 18, 0) });
        }
        Add(scroller = new ScrollView { Id = "SketchFeed", Direction = ScrollDirection.Vertical,
            ScrollbarEndInset = 0, Content = stack });
    }
    protected override Size MeasureCore(Size available, IMeasureContext context) { scroller.Measure(available, context); return available; }
    protected override void ArrangeCore(Rect rect, IMeasureContext context) => scroller.Arrange(rect, context);

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        var active = false;
        var dt = NFloat.Max(0, (NFloat)(current - previous).TotalSeconds);
        foreach (ref var bubble in tapBubbles.AsSpan())
        {
            if (!bubble.Visible) continue;
            bubble.Age = bubble.Age < 0 ? 0 : bubble.Age + dt;
            bubble.Visible = bubble.Age < 1.2f;
            active |= bubble.Visible;
            InvalidateRender();
        }
        if (active) RequestAnimationFrame();
    }

    protected override void RenderCore(IContext context)
    {
        base.RenderCore(context);
        // Paint above the feed so scrolling content cannot cover the tap feedback.
        foreach (ref var bubble in tapBubbles.AsSpan())
        {
            if (!bubble.Visible) continue;
            var t = NFloat.Clamp(bubble.Age / 1.2f, 0, 1);
            var center = bubble.Origin + new Vector(0, -18 - 64 * Easing.EaseOutCubic(t));
            context.Save();
            context.GlobalAlpha = 1 - Easing.EaseInCubic(t);
            InputDrawing.Panel(context, new Rect(center.X - 48, center.Y - 17, 96, 34), Teal, 17);
            InputDrawing.Text(context, "Tapped!", center, 14, Colors.White, true, TextAlign.Center);
            context.Restore();
        }
    }

    protected override void OnDeactivate()
    {
        Array.Clear(tapBubbles);
        base.OnDeactivate();
    }
}
