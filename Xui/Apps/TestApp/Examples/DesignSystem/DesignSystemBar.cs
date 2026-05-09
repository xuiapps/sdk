using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// A simple bar that renders Primary background with a label. Used for header/footer.
/// </summary>
internal class DesignSystemBar : View
{
    private readonly Label label;
    private readonly NFloat padding = 12;

    public DesignSystemBar(string text, NFloat fontSize)
    {
        label = new Label { Text = text, FontSize = fontSize, FontWeight = Core.Canvas.FontWeight.SemiBold };
        this.AddProtectedChild(label);
    }

    public override int Count => 1;
    public override View this[int index] => index == 0 ? label : throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var inner = new Size(available.Width - padding * 2, available.Height - padding * 2);
        var labelSize = label.Measure(inner, context);
        return new Size(available.Width, labelSize.Height + padding * 2);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var inner = new Size(rect.Width - padding * 2, rect.Height - padding * 2);
        var labelSize = label.Measure(inner, context);
        label.Arrange(new Rect(
            rect.X + padding,
            rect.Y + (rect.Height - labelSize.Height) / 2,
            inner.Width,
            labelSize.Height), context);
    }

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        if (ds != null)
        {
            context.SetFill(ds.Colors.Primary.Background);
            context.FillRect(this.Frame);

            label.TextColor = ds.Colors.Primary.Foreground;
        }

        base.RenderCore(context);
    }
}
