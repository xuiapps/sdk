using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.DevKit.UI.Widgets;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

internal enum MobileHomeAction { Heart, Fps, Star }

/// <summary>Uses the shared button's tap/scroll negotiation and delayed press feedback.</summary>
internal sealed class MobileHomeActionButton : Button
{
    private readonly MobileHomeAction action;
    private bool selected;
    public bool Selected
    {
        get => selected;
        set { if (selected != value) { selected = value; InvalidateRender(); } }
    }

    public MobileHomeActionButton(string id, MobileHomeAction action)
    {
        Id = id;
        this.action = action;
        Variant = ButtonVariant.Text;
        HorizontalAlignment = Xui.Core.UI.HorizontalAlignment.Stretch;
        VerticalAlignment = Xui.Core.UI.VerticalAlignment.Stretch;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) =>
        (action == MobileHomeAction.Fps ? 56 : 48, 48);

    protected override void RenderCore(IContext context)
    {
        var frame = Frame.Expand(-3);
        if (action != MobileHomeAction.Star)
        {
            context.BeginPath(); context.RoundRect(frame, frame.Height / 2);
            context.SetFill(new Color(0xEEF0F4FF)); context.Fill();
        }
        base.RenderCore(context);
        var center = Frame.Center;
        context.SetStroke(new Color(0x242529FF));
        context.LineWidth = 2;
        context.LineJoin = LineJoin.Round;
        if (action == MobileHomeAction.Fps)
        {
            context.SetFont(new Font(12, "Inter", FontWeight.SemiBold));
            context.TextAlign = TextAlign.Center; context.TextBaseline = TextBaseline.Middle;
            context.SetFill(new Color(0x242529FF)); context.FillText("FPS", center);
        }
        else if (action == MobileHomeAction.Heart)
        {
            context.BeginPath();
            context.MoveTo((center.X, center.Y + 10));
            context.CurveTo((center.X - 22, center.Y - 4), (center.X - 8, center.Y - 18), (center.X, center.Y - 7));
            context.CurveTo((center.X + 8, center.Y - 18), (center.X + 22, center.Y - 4), (center.X, center.Y + 10));
            context.ClosePath(); context.Stroke();
        }
        else
        {
            context.BeginPath();
            for (var i = 0; i < 10; i++)
            {
                NFloat angle = NFloat.Pi * i / 5 - NFloat.Pi / 2;
                NFloat radius = i % 2 == 0 ? 15 : 7;
                var point = center + new Vector(NFloat.Cos(angle) * radius, NFloat.Sin(angle) * radius);
                if (i == 0) context.MoveTo(point); else context.LineTo(point);
            }
            context.ClosePath();
            if (Selected) { context.SetFill(new Color(0xE5A321FF)); context.Fill(); }
            context.SetStroke(new Color(Selected ? 0xB77C0AFFu : 0x7E8590FFu)); context.Stroke();
        }
    }
}
