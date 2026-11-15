using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// A thin hue ring surrounding a white/black/hue shade triangle.
/// </summary>
internal class ColorWheelView : View
{
    private IImage? wheelImage;
    private IImage? triangleImage;
    private int pointer = -1;
    private bool draggingShade;

    private static readonly NFloat WheelSize = 230;
    private static readonly NFloat InnerFraction = 0.83f;
    private static readonly NFloat HueHitWidth = 48;
    private static readonly NFloat IndicatorRadius = 8;
    private static readonly NFloat SmallIndicatorRadius = 5;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    protected override void OnActivate()
    {
        base.OnActivate();
        wheelImage = this.GetService<IImage>();
        wheelImage?.Load("asset://ColorWheel.png");
        triangleImage = this.GetService<IImage>();
        triangleImage?.Load("asset://ColorTriangle.png");
    }

    protected override void OnDeactivate()
    {
        wheelImage = triangleImage = null;
        pointer = -1;
        base.OnDeactivate();
    }

    private (Point center, NFloat outerR, NFloat innerR) WheelGeometry()
    {
        var outerR = NFloat.Max(0, NFloat.Min(WheelSize, this.Frame.Width - 16)) / 2;
        var cx = this.Frame.X + 8 + outerR;
        var cy = this.Frame.Y + 8 + outerR;
        return (new Point(cx, cy), outerR, outerR * InnerFraction);
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Tunnel && e.Type != PointerEventType.LostCapture) return;
        var (center, outerR, innerR) = WheelGeometry();
        if (outerR <= 16) return;
        var dx = e.State.Position.X - center.X;
        var dy = e.State.Position.Y - center.Y;
        if (e.Type == PointerEventType.Down && pointer < 0)
        {
            var dist = NFloat.Sqrt(dx * dx + dy * dy);
            var triangle = TriangleBounds(center, innerR);
            var (white, black) = Weights(triangle, e.State.Position);
            draggingShade = white >= 0 && black >= 0 && white + black <= 1;
            // Use the existing 8-point outer margin and extend inward for thumbs.
            // The triangle keeps priority wherever the touch regions overlap.
            var hitOuterR = outerR + 8;
            var hitInnerR = NFloat.Max(0, hitOuterR - HueHitWidth);
            if (!draggingShade && (dist < hitInnerR || dist > hitOuterR)) return;
            pointer = e.PointerId;
            CapturePointer(pointer, PointerGestures.Drag);
            if (draggingShade) UpdateShade(triangle, e.State.Position);
            else UpdateHue(dx, dy);
        }
        else if (e.PointerId == pointer)
        {
            if (e.Type == PointerEventType.Move)
            {
                if (draggingShade) UpdateShade(TriangleBounds(center, innerR), e.State.Position);
                else UpdateHue(dx, dy);
            }
            else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
            {
                pointer = -1;
                if (e.Type != PointerEventType.LostCapture) ReleasePointer(e.PointerId);
            }
        }
    }

    internal static Color HueColor(NFloat hue) => new Oklch(0.70f, Oklch.MaxSrgbChroma(0.70f, hue), hue).ToColor();

    private static Rect TriangleBounds(Point center, NFloat innerR)
    {
        var radius = NFloat.Max(0, innerR - 12);
        var halfHeight = NFloat.Sqrt(3) * radius / 2;
        return new Rect(center.X - radius / 2, center.Y - halfHeight, radius * 1.5f, halfHeight * 2);
    }

    private static (NFloat white, NFloat black) Weights(Rect triangle, Point point)
    {
        var hue = (point.X - triangle.Left) / triangle.Width;
        var white = (1 - hue) / 2 - (point.Y - triangle.Center.Y) / triangle.Height;
        return (white, 1 - hue - white);
    }

    private void UpdateShade(Rect triangle, Point point)
    {
        var (white, black) = Weights(triangle, point);
        if (white < 0 || black < 0 || white + black > 1)
        {
            // Project to the nearest edge, preserving smooth drags outside the triangle.
            var a = triangle.TopLeft;
            var b = new Point(triangle.Left, triangle.Bottom);
            var c = new Point(triangle.Right, triangle.Center.Y);
            var ab = Closest(point, a, b);
            var bc = Closest(point, b, c);
            var ca = Closest(point, c, a);
            var nearest = DistanceSquared(point, ab) < DistanceSquared(point, bc) ? ab : bc;
            if (DistanceSquared(point, ca) < DistanceSquared(point, nearest)) nearest = ca;
            (white, black) = Weights(triangle, nearest);
        }
        this.GetService<IDesignSystemEditor>()?.SetShade(white, black);
    }

    private static NFloat DistanceSquared(Point a, Point b) => (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);

    private static Point Closest(Point p, Point a, Point b)
    {
        var ab = b - a;
        var t = NFloat.Clamp(((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / DistanceSquared(a, b), 0, 1);
        return a + ab * t;
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

    protected override Size MeasureCore(Size available, IMeasureContext context) => new Size(NFloat.Min(WheelSize + 16, available.Width), NFloat.Min(WheelSize + 16, available.Width));

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
            context.Scale(new Vector(outerR * 2 / wheelImage.Size.Width, outerR * 2 / wheelImage.Size.Height));

            context.SetFill(new ImagePattern(wheelImage, PatternRepeat.NoRepeat));
            context.BeginPath();
            context.Arc(
                new Point(wheelImage.Size.Width / 2, wheelImage.Size.Height / 2),
                wheelImage.Size.Width / 2,
                0, twoPi);
            context.MoveTo((wheelImage.Size.Width / 2 + wheelImage.Size.Width / 2 * InnerFraction, wheelImage.Size.Height / 2));
            context.Arc((wheelImage.Size.Width / 2, wheelImage.Size.Height / 2), wheelImage.Size.Width / 2 * InnerFraction, 0, twoPi);
            context.Fill(FillRule.EvenOdd);
            context.Restore();

        }

        var triangle = TriangleBounds(center, innerR);
        context.Save();
        context.BeginPath();
        context.MoveTo(triangle.TopLeft);
        context.LineTo((triangle.Right, triangle.Center.Y));
        context.LineTo((triangle.Left, triangle.Bottom));
        context.ClosePath();
        context.Clip();
        context.SetFill(HueColor(editor.PrimaryHue));
        context.FillRect(triangle);
        if (triangleImage is not null && triangleImage.Size != Size.Empty)
        {
            context.Translate(new Vector(triangle.X, triangle.Y));
            context.Scale(new Vector(triangle.Width / triangleImage.Size.Width, triangle.Height / triangleImage.Size.Height));
            context.SetFill(new ImagePattern(triangleImage, PatternRepeat.NoRepeat));
            context.FillRect(new Rect(0, 0, triangleImage.Size.Width, triangleImage.Size.Height));
        }
        context.Restore();
        var hueWeight = 1 - editor.WhiteWeight - editor.BlackWeight;
        var selectedPoint = new Point(triangle.Left + hueWeight * triangle.Width,
            triangle.Top + (editor.BlackWeight + hueWeight / 2) * triangle.Height);
        DrawHueIndicator(context, selectedPoint, 0, 0, 6, ds.Colors.Primary.Background, true);

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
