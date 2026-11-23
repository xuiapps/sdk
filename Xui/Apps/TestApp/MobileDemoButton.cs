using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Widgets;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>A full-width, token-driven mobile navigation row for a TestApp demo.</summary>
internal sealed class MobileDemoButton<TExample> : Button
    where TExample : Example, new()
{
    public MobileDemoIcon Icon { get; init; }

    protected override Rect LabelBounds => new(Frame.X + 44, Frame.Y,
        NFloat.Max(0, Frame.Width - 68), Frame.Height);

    public MobileDemoButton()
    {
        Variant = ButtonVariant.Text;
        Role = ColorRole.Neutral;
        LabelTextAlign = TextAlign.Left;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        MinimumHeight = 44;
        WaitForPressFeedback = true;
        Clicked = NavigateToExample;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var size = base.MeasureCore(available, context);
        return new Size(size.Width + 68, NFloat.Max(44, size.Height));
    }

    protected override void RenderCore(IContext context)
    {
        base.RenderCore(context);
        var inset = (GetService(typeof(IDesignSystem)) as IDesignSystem)?.Spacing.Active.L ?? 0;
        MobileDemoIcons.Draw(context, (Frame.X + inset, Frame.Center.Y - 16), Icon);
        context.Save();
        context.SetStroke(XuiLogo.AccentColor);
        context.LineWidth = 2;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        var x = Frame.Right - inset - 10;
        var y = Frame.Center.Y;
        context.BeginPath();
        context.MoveTo((x - 3, y - 5));
        context.LineTo((x + 2, y));
        context.LineTo((x - 3, y + 5));
        context.Stroke();
        context.Restore();
    }

    private void NavigateToExample()
    {
        this.TryFindExampleNavigation(out var navigation);
        navigation?.PushExamplePage<TExample>();
    }
}
