using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.XuiSDK.Pages;

public class ScrollDemoPage : View
{
    private readonly ScrollView scrollView;

    public ScrollDemoPage()
    {
        var stack = new VerticalStack();
        for (int i = 1; i <= 40; i++)
        {
            stack.Add(new RowItem { Number = i });
        }

        scrollView = new ScrollView { Content = stack };
        AddProtectedChild(scrollView);
    }

    public override int Count => 1;
    public override View this[int index] => index == 0 ? scrollView : throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        scrollView.Measure(available, context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        scrollView.Arrange(rect, context);
    }

    protected override void RenderCore(IContext context)
    {
        base.RenderCore(context);
    }

    private class RowItem : View
    {
        public int Number { get; set; }

        private static readonly nfloat RowHeight = 52f;

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            return (available.Width, RowHeight);
        }

        protected override void RenderCore(IContext context)
        {
            var rect = this.Frame;

            // Row background (alternating)
            if (Number % 2 == 0)
            {
                context.SetFill(new Color(0xF8F8F8FF));
                context.FillRect(rect);
            }

            // Separator line
            context.SetStroke(new Color(0x00000012));
            context.LineWidth = 1;
            context.BeginPath();
            context.MoveTo(new Point(rect.X + 16, rect.Bottom));
            context.LineTo(new Point(rect.Right, rect.Bottom));
            context.Stroke();

            // Row number badge
            nfloat badgeSize = 32;
            nfloat badgeX = rect.X + 16;
            nfloat badgeY = rect.Y + (RowHeight - badgeSize) / 2;
            context.SetFill(new Color(0x0066CCFF));
            context.BeginPath();
            context.RoundRect(new Rect(badgeX, badgeY, badgeSize, badgeSize), 8);
            context.Fill(FillRule.NonZero);

            context.SetFont(new Font(13, ["Segoe UI"], fontWeight: FontWeight.SemiBold));
            context.TextBaseline = TextBaseline.Middle;
            context.TextAlign = TextAlign.Center;
            context.SetFill(Colors.White);
            context.FillText(Number.ToString(), new Point(badgeX + badgeSize / 2, rect.Y + RowHeight / 2));

            // Row label
            context.SetFont(new Font(14, ["Segoe UI"]));
            context.TextAlign = TextAlign.Left;
            context.TextBaseline = TextBaseline.Middle;
            context.SetFill(new Color(0x1A1A1AFF));
            context.FillText($"Item {Number}", new Point(badgeX + badgeSize + 12, rect.Y + RowHeight / 2));
        }
    }
}
