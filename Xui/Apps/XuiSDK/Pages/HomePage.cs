using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.XuiSDK.Pages;

public class HomePage : ViewCollection
{
    private readonly FeatureCard[] cards;
    private readonly ImageView banner;

    public HomePage()
    {
        cards =
        [
            new FeatureCard { Title = "Cross-Platform", Line1 = "Windows, macOS, iOS,", Line2 = "Android, Web" },
            new FeatureCard { Title = "Native Rendering", Line1 = "Hardware-accelerated", Line2 = "2D canvas API" },
            new FeatureCard { Title = "Lightweight", Line1 = "Minimal overhead,", Line2 = "maximum performance" },
            new FeatureCard { Title = "Flexible Layout", Line1 = "Stack, grid, and", Line2 = "custom layouts" },
            new FeatureCard { Title = "Input System", Line1 = "Pointer, touch, and", Line2 = "keyboard events" },
            new FeatureCard { Title = "Open Source", Line1 = "MIT licensed,", Line2 = "community driven" },
        ];

        foreach (var card in cards)
            Add(card);

        banner = new ImageView { Source = "Assets/test.png" };
        Add(banner);
    }

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        var cardSize = new Size(180, 120);
        foreach (var card in cards)
            card.Measure(cardSize, context);

        banner.Measure(new Size(200, 80), context);

        return availableBorderEdgeSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        nfloat startX = rect.X + 40;
        nfloat startY = rect.Y + 130;
        nfloat cardW = 180;
        nfloat cardH = 120;
        nfloat gapX = 20;
        nfloat gapY = 20;

        for (int i = 0; i < cards.Length; i++)
        {
            int col = i % 3;
            int row = i / 3;
            nfloat x = startX + col * (cardW + gapX);
            nfloat y = startY + row * (cardH + gapY);
            cards[i].Arrange(new Rect(x, y, cardW, cardH), context);
        }

        // Banner image: fixed 200x80 slot in the top-right corner
        banner.Arrange(new Rect(rect.Right - 240, rect.Y + 40, 200, 80), context);
    }

    protected override void RenderCore(IContext context)
    {
        var rect = this.Frame;

        // Title
        context.SetFont(new Font(32, ["Segoe UI"], fontWeight: FontWeight.Light));
        context.TextBaseline = TextBaseline.Top;
        context.TextAlign = TextAlign.Left;
        context.SetFill(new Color(0x1A1A1AFF));
        context.FillText("Welcome to Xui", new Point(rect.X + 40, rect.Y + 40));

        // Subtitle
        context.SetFont(new Font(14, ["Segoe UI"]));
        context.SetFill(new Color(0x606060FF));
        context.FillText("Cross-platform UI framework for .NET", new Point(rect.X + 40, rect.Y + 82));

        // Render child cards
        base.RenderCore(context);
    }
}
