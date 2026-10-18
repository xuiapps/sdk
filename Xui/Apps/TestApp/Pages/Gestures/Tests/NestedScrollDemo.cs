using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using NFloat = System.Runtime.InteropServices.NFloat;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;

namespace Xui.Apps.TestApp.Pages.Gestures.Tests;

/// <summary>Horizontal destination shelves nested inside a vertical feed.</summary>
public class NestedScrollDemo : ViewCollection
{
    private readonly ScrollView outer;
    public NestedScrollDemo()
    {
        var inset = InputDemoCard.ContentInset;
        var feed = new VerticalStack { Margin = (inset, 0, inset, 0) };
        var groups = new[] { "WEEKEND WANDERS", "SLOW MORNINGS", "AFTER HOURS", "THE GREAT OUTDOORS" };
        var places = new[] { "Coastal walk", "Forest cabin", "Old town", "By the lake", "Mountain air", "City garden" };
        for (var row = 0; row < groups.Length; row++)
        {
            var heading = Text(groups[row], 10, FontWeight.SemiBold, Teal);
            heading.Margin = (row == 0 ? 0 : 22, inset, 10, inset); feed.Add(heading);
            var shelf = new HorizontalStack { Margin = (0, inset, 0, inset) };
            for (var col = 0; col < places.Length; col++)
            {
                var content = new VerticalStack();
                content.Add(new LandscapeTile(row + col) { MinimumHeight = 72 });
                var title = Text(places[(row + col) % places.Length], 13, FontWeight.SemiBold);
                title.Margin = (10, 0, 4, 0); content.Add(title);
                content.Add(Text(col % 2 == 0 ? "A little closer to nature" : "Take the scenic route", 9, color: Muted));
                shelf.Add(new Border { MinimumWidth = 152, MaximumWidth = 152, Padding = 12,
                    CornerRadius = 14, BackgroundColor = Colors.White,
                    Margin = (0, col == places.Length - 1 ? 0 : 10, 0, 0), Content = content });
            }
            feed.Add(new ScrollView { Id = $"DestinationShelf{row}", Direction = ScrollDirection.Horizontal,
                ScrollbarEndInset = inset, MinimumHeight = 150, MaximumHeight = 150, Content = shelf });
        }
        Add(outer = new ScrollView { Id = "DestinationFeed", Direction = ScrollDirection.Vertical,
            ScrollbarEndInset = 0, Content = feed });
    }
    protected override Size MeasureCore(Size available, IMeasureContext context) { outer.Measure(available, context); return available; }
    protected override void ArrangeCore(Rect rect, IMeasureContext context) => outer.Arrange(rect, context);

    private sealed class LandscapeTile(int variation) : View
    {
        protected override void RenderCore(IContext ctx)
        {
            var teal = variation % 2 == 0;
            Widgets.InputDrawing.Panel(ctx, Frame, teal ? PaleTeal : PaleBlue, 9);
            Widgets.InputDrawing.Clip(ctx, Frame);
            ctx.SetFill(new Color(0xF0C47AFF)); ctx.BeginPath();
            ctx.Arc((Frame.Right - 24, Frame.Y + 20), 10, 0, NFloat.Pi * 2); ctx.Fill();
            ctx.SetFill(teal ? new Color(0x9CCDBAFF) : new Color(0xADB8E9FF)); ctx.BeginPath();
            ctx.MoveTo((Frame.X, Frame.Bottom)); ctx.LineTo((Frame.X, Frame.Y + 45));
            ctx.LineTo((Frame.X + 45, Frame.Y + 20)); ctx.LineTo((Frame.Right, Frame.Bottom)); ctx.ClosePath(); ctx.Fill();
            ctx.SetFill(teal ? Teal : Blue); ctx.BeginPath(); ctx.MoveTo((Frame.X + 20, Frame.Bottom));
            ctx.LineTo((Frame.Right - 20, Frame.Y + 36)); ctx.LineTo(Frame.BottomRight); ctx.ClosePath(); ctx.Fill();
        }
    }
}
