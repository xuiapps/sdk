using System.Runtime.InteropServices;

using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// The color wheel with annular ring and hue indicators.
/// </summary>
internal class ColorWheelView : View
{
    private IImage? wheelImage;
    private bool dragging;

    private static readonly NFloat WheelSize = 230;
    private static readonly NFloat InnerFraction = 0.35f;
    private static readonly NFloat IndicatorRadius = 8;
    private static readonly NFloat SmallIndicatorRadius = 5;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    protected override void OnActivate()
    {
        base.OnActivate();
        wheelImage = this.GetService<IImage>();
        wheelImage?.Load("Assets/ColorWheel.png");
    }

    protected override void OnDeactivate() => wheelImage = null;

    private (Point center, NFloat outerR, NFloat innerR) WheelGeometry()
    {
        var outerR = WheelSize / 2;
        var cx = this.Frame.X + 8 + outerR;
        var cy = this.Frame.Y + 8 + outerR;
        return (new Point(cx, cy), outerR, outerR * InnerFraction);
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Tunnel)
        {
            base.OnPointerEvent(ref e, phase);
            return;
        }

        if (e.Type == PointerEventType.Down)
        {
            var (center, outerR, innerR) = WheelGeometry();
            var dx = e.State.Position.X - center.X;
            var dy = e.State.Position.Y - center.Y;
            var dist = NFloat.Sqrt(dx * dx + dy * dy);

            if (dist >= innerR && dist <= outerR)
            {
                UpdateHue(dx, dy);
                dragging = true;
                this.CapturePointer(e.PointerId);
            }
        }
        else if (e.Type == PointerEventType.Move && dragging)
        {
            var (center, _, _) = WheelGeometry();
            UpdateHue(e.State.Position.X - center.X, e.State.Position.Y - center.Y);
        }
        else if (e.Type == PointerEventType.Up && dragging)
        {
            dragging = false;
            this.ReleasePointer(e.PointerId);
        }

        base.OnPointerEvent(ref e, phase);
    }

    private void UpdateHue(NFloat dx, NFloat dy)
    {
        var editor = this.GetService<IDesignSystemEditor>();
        if (editor == null) return;
        var angle = NFloat.Atan2(dy, dx);
        var hue = (NFloat)(angle * (180.0 / Math.PI));
        if (hue < 0) hue += 360;
        editor.SetHue(hue);
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) => new Size(WheelSize + 16, WheelSize + 16);

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        var editor = this.GetService<IDesignSystemEditor>();
        if (ds == null || editor == null) return;

        var twoPi = (NFloat)(2 * Math.PI);
        var (center, outerR, innerR) = WheelGeometry();

        if (wheelImage != null && wheelImage.Size != Size.Empty)
        {
            context.Save();
            context.Translate(new Vector(center.X - outerR, center.Y - outerR));
            context.Scale(new Vector(WheelSize / wheelImage.Size.Width, WheelSize / wheelImage.Size.Height));

            context.SetFill(new ImagePattern(wheelImage, PatternRepeat.NoRepeat));
            context.BeginPath();
            context.Arc(
                new Point(wheelImage.Size.Width / 2, wheelImage.Size.Height / 2),
                wheelImage.Size.Width / 2,
                0, twoPi);
            context.Fill(FillRule.NonZero);
            context.Restore();

            // Punch out inner circle
            context.BeginPath();
            context.Arc(center, innerR, 0, twoPi);
            context.SetFill(ds.Colors.Application.Background);
            context.Fill(FillRule.NonZero);
        }

        // Ring strokes
        context.SetStroke(new Color(0x99999966));
        context.LineWidth = 1;
        context.BeginPath();
        context.Arc(center, outerR, 0, twoPi);
        context.Stroke();
        context.BeginPath();
        context.Arc(center, innerR, 0, twoPi);
        context.Stroke();

        // Hue indicators
        var indicatorDist = (outerR + innerR) / 2;

        DrawHueIndicator(context, center, indicatorDist, editor.PrimaryHue,
            IndicatorRadius, ds.Colors.Primary.Background, true);

        var (secHue, tertHue) = GetDerivedHues(editor.PrimaryHue, editor.Harmony);
        DrawHueIndicator(context, center, indicatorDist, secHue,
            SmallIndicatorRadius, ds.Colors.Secondary.Background, false);
        DrawHueIndicator(context, center, indicatorDist, tertHue,
            SmallIndicatorRadius, ds.Colors.Tertiary.Background, false);
    }

    private static void DrawHueIndicator(IContext context, Point center, NFloat dist,
        NFloat hueDegrees, NFloat radius, Color fillColor, bool isPrimary)
    {
        var hueRad = (NFloat)(hueDegrees * Math.PI / 180.0);
        var pos = new Point(
            center.X + NFloat.Cos(hueRad) * dist,
            center.Y + NFloat.Sin(hueRad) * dist);
        var twoPi = (NFloat)(2 * Math.PI);

        context.BeginPath();
        context.Arc(pos, radius, 0, twoPi);
        context.SetFill(fillColor);
        context.Fill(FillRule.NonZero);

        context.BeginPath();
        context.Arc(pos, radius, 0, twoPi);
        context.SetStroke(new Color(0xFFFFFFFF));
        context.LineWidth = isPrimary ? 3 : 2;
        context.Stroke();

        context.BeginPath();
        context.Arc(pos, radius + (isPrimary ? 2 : 1.5f), 0, twoPi);
        context.SetStroke(new Color(0x00000066));
        context.LineWidth = 1;
        context.Stroke();
    }

    internal static (NFloat secondary, NFloat tertiary) GetDerivedHues(NFloat primary, ColorHarmony harmony)
    {
        NFloat s, t;
        switch (harmony)
        {
            case ColorHarmony.Complementary:      s = primary + 180; t = primary + 180; break;
            case ColorHarmony.Analogous:          s = primary + 30;  t = primary + 60;  break;
            case ColorHarmony.SplitComplementary:  s = primary + 150; t = primary + 210; break;
            case ColorHarmony.Triadic:            s = primary + 120; t = primary + 240; break;
            case ColorHarmony.Tetradic:           s = primary + 90;  t = primary + 180; break;
            default:                              s = primary + 150; t = primary + 210; break;
        }
        s %= 360; if (s < 0) s += 360;
        t %= 360; if (t < 0) t += 360;
        return (s, t);
    }
}
