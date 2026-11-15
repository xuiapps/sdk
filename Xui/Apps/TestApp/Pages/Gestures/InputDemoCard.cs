using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Gestures;

internal sealed class InputDemoCard : ViewCollection
{
    internal static readonly NFloat ContentInset = 28;
    private static readonly NFloat HeaderHeight = 56;
    private static readonly NFloat DetailsHeight = 132;
    private readonly Label category;
    private readonly VerticalStack intro;
    private readonly View body;
    private readonly Label footer;
    private readonly bool edgeToEdge;

    public InputDemoCard(string category, string title, string detail, string hint, View body, bool edgeToEdge = false)
    {
        this.edgeToEdge = edgeToEdge;
        Id = body.GetType().Name + ".Card";
        Add(this.category = Text(category, 11, FontWeight.SemiBold, Teal));
        this.category.Id = body.GetType().Name + ".Category";
        Add(this.body = body);
        Add(intro = new VerticalStack { Id = body.GetType().Name + ".Details" });
        intro.Add(Text(title, 24, FontWeight.SemiBold));
        var description = Text(detail, 11, color: Muted); description.Margin = (8, 0, 4, 0);
        intro.Add(description);
        intro.Add(Text(hint, 11, color: Muted));
        Add(footer = Text("←  Swipe here to explore  →", 10, color: Muted));
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var width = NFloat.Max(1, available.Width - ContentInset * 2);
        category.Measure((width, 16), context);
        intro.Measure((width, 84), context);
        body.Measure((edgeToEdge ? available.Width : width,
            NFloat.Max(1, available.Height - HeaderHeight - DetailsHeight)), context);
        footer.Measure((width, 20), context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var height = NFloat.Max(1, rect.Height - HeaderHeight - DetailsHeight);
        var width = NFloat.Max(1, rect.Width - ContentInset * 2);
        category.Arrange((rect.X + ContentInset, rect.Y + ContentInset, width, 16), context);
        body.Arrange(edgeToEdge ? new Rect(rect.X, rect.Y + HeaderHeight, rect.Width, height)
            : new Rect(rect.X + ContentInset, rect.Y + HeaderHeight, width, height), context);
        intro.Arrange((rect.X + ContentInset, rect.Y + HeaderHeight + height + 16, width, 84), context);
        footer.Arrange((rect.X + ContentInset, rect.Bottom - 28, width, 14), context);
    }
}
