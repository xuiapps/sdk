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
    private NFloat backgroundOpacity = 1;
    private string frameRate = "--";
    private int? displayedFrameRate;
    private MobileFrameRate? warmup;
    private TimeSpan? warmupStart;
    private NFloat warmupProgress;

    public void StartWarmup(MobileFrameRate source)
    {
        warmup = source;
        warmupStart = null;
        RequestAnimationFrame();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (warmup is null) return;
        if (warmupStart is null)
        {
            warmupStart = current;
            // Exclude startup/layout work before the first visible animation tick.
            warmup.BeginWarmup();
        }
        warmupProgress = NFloat.Clamp((NFloat)(current - warmupStart.Value).TotalSeconds / 2f, 0, 1);
        InvalidateRender();
        if (warmupProgress < 1) RequestAnimationFrame();
        else
        {
            var source = warmup;
            warmup = null;
            source.EndWarmup();
        }
    }

    public NFloat BackgroundOpacity
    {
        get => backgroundOpacity;
        set => SetViewRenderProperty(ref backgroundOpacity, value);
    }

    public void SetFrameRate(int value)
    {
        if (displayedFrameRate == value) return;
        displayedFrameRate = value;
        frameRate = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        InvalidateRender();
    }

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
        if (action != MobileHomeAction.Star && backgroundOpacity > 0)
        {
            context.BeginPath(); context.RoundRect(frame, frame.Height / 2);
            var background = new Color(0xEEF0F4FF) { Alpha = backgroundOpacity };
            context.SetFill(background); context.Fill();
        }
        base.RenderCore(context);
        var center = Frame.Center;
        context.SetStroke(new Color(0x242529FF));
        context.LineWidth = 2;
        context.LineJoin = LineJoin.Round;
        if (action == MobileHomeAction.Fps)
        {
            context.SetFont(new Font(17, "Inter", FontWeight.SemiBold));
            context.TextAlign = TextAlign.Center; context.TextBaseline = TextBaseline.Middle;
            context.SetFill(new Color(0x242529FF));
            if (warmup is not null)
            {
                context.BeginPath();
                var angle = warmupProgress * NFloat.Pi * 4;
                context.Arc((center.X, center.Y - 6), 6, angle, angle + NFloat.Pi * 1.4f);
                context.LineCap = LineCap.Round;
                context.Stroke();
            }
            else context.FillText(frameRate, (center.X, center.Y - 6));
            context.SetFont(new Font(9, "Inter", FontWeight.Medium));
            context.FillText("fps", (center.X, center.Y + 10));
        }
        else if (action == MobileHomeAction.Heart)
        {
            context.Save();
            context.Translate(center);
            context.Scale((0.82f, 0.82f));
            center = (0, 0);
            context.BeginPath();
            context.MoveTo((center.X, center.Y + 10));
            context.CurveTo((center.X - 22, center.Y - 4), (center.X - 8, center.Y - 18), (center.X, center.Y - 7));
            context.CurveTo((center.X + 8, center.Y - 18), (center.X + 22, center.Y - 4), (center.X, center.Y + 10));
            context.ClosePath(); context.Stroke();
            context.Restore();
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
