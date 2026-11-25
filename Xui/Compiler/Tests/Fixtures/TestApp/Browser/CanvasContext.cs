using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;

namespace TestAppCanvas;

// Browser-shell Canvas boundary. Unsupported surface effects remain explicit errors.
public sealed class CanvasContext : IContext
{
    public void Save() => NativeCanvas.Command("Save");
    public void Restore() => NativeCanvas.Command("Restore");
    public NFloat GlobalAlpha { set => NativeCanvas.Command("GlobalAlpha", (double)value); }
    public LineCap LineCap { set => NativeCanvas.Command("LineCap", (int)value); }
    public LineJoin LineJoin { set => NativeCanvas.Command("LineJoin", (int)value); }
    public NFloat LineWidth { set => NativeCanvas.Command("LineWidth", (double)value); }
    public NFloat MiterLimit { set => NativeCanvas.Command("MiterLimit", (double)value); }
    public NFloat LineDashOffset { set => NativeCanvas.Command("LineDashOffset", (double)value); }
    public TextAlign TextAlign { set => NativeCanvas.Command("TextAlign", (int)value); }
    public TextBaseline TextBaseline { set => NativeCanvas.Command("TextBaseline", (int)value); }
    public void SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        NativeCanvas.Command("DashBegin");
        for (int i=0; i<segments.Length; i++) NativeCanvas.Command("DashValue", (double)segments[i]);
        NativeCanvas.Command("DashEnd");
    }
    public void SetStroke(Color color) => NativeCanvas.Command("SetStroke", (double)color.Red, (double)color.Green, (double)color.Blue, (double)color.Alpha);
    public void SetFill(Color color) => NativeCanvas.Command("SetFill", (double)color.Red, (double)color.Green, (double)color.Blue, (double)color.Alpha);
    public void FillRect(Rect rect) => NativeCanvas.Command("FillRect", (double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height);
    public void SetFont(Font font)
    {
        NativeCanvas.Command("Font", (double)font.FontSize, (double)font.FontWeight.Value, (double)font.FontStretch.Value, (double)font.LineHeight, font.FontFamily);
        NativeCanvas.Command("FontStyle", font.FontStyle.IsItalic ? 1 : 0, font.FontStyle.IsOblique ? 1 : 0, (double)font.FontStyle.ObliqueAngle);
    }
    public void BeginPath() => NativeCanvas.Command("BeginPath");
    public void Dispose() { }
    public TextMetrics MeasureText(string text) => new(
        new LineMetrics((NFloat)NativeCanvas.Measure(text,0), (NFloat)NativeCanvas.Measure(text,1),
            (NFloat)NativeCanvas.Measure(text,2), (NFloat)NativeCanvas.Measure(text,3), (NFloat)NativeCanvas.Measure(text,4)),
        new FontMetrics((NFloat)NativeCanvas.Measure(text,5), (NFloat)NativeCanvas.Measure(text,6),
            (NFloat)NativeCanvas.Measure(text,7), (NFloat)NativeCanvas.Measure(text,8),
            (NFloat)NativeCanvas.Measure(text,9), (NFloat)NativeCanvas.Measure(text,10), (NFloat)NativeCanvas.Measure(text,11)));
    public void FillText(string text, Point pos) => NativeCanvas.Command("FillText", (double)pos.X, (double)pos.Y, text:text);
    public void StrokeRect(Rect rect) => NativeCanvas.Command("StrokeRect", (double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height);
    public void SetStroke(LinearGradient value) { LinearBrush(value); NativeCanvas.Command("StrokeGradient"); }
    public void SetStroke(RadialGradient value) => throw new InvalidOperationException("Unsupported Canvas operation: SetStroke(RadialGradient value).");
    public void SetStroke(ImagePattern value) => throw new InvalidOperationException("Unsupported Canvas operation: SetStroke(ImagePattern value).");
    public void SetFill(LinearGradient value) { LinearBrush(value); NativeCanvas.Command("FillGradient"); }
    private static void LinearBrush(LinearGradient value)
    {
        NativeCanvas.Command("LinearGradient", (double)value.StartPoint.X, (double)value.StartPoint.Y, (double)value.EndPoint.X, (double)value.EndPoint.Y);
        for (int i = 0; i < value.GradientStops.Length; i++)
        {
            var stop = value.GradientStops[i];
            NativeCanvas.Command("GradientColor", (double)stop.Color.Red, (double)stop.Color.Green, (double)stop.Color.Blue, (double)stop.Color.Alpha);
            NativeCanvas.Command("GradientStop", (double)stop.Offset);
        }
    }
    public void SetFill(RadialGradient value) => throw new InvalidOperationException("Unsupported Canvas operation: SetFill(RadialGradient value).");
    public void SetFill(ImagePattern value) => throw new InvalidOperationException("Unsupported Canvas operation: SetFill(ImagePattern value).");
    public void SetFilter() => NativeCanvas.Command("ClearFilter");
    public void SetFilter(Brightness value) => throw new InvalidOperationException("Unsupported Canvas operation: SetFilter(Brightness value).");
    public CompositionGroup Group(scoped in GroupOptions options)
    {
        Save();
        try
        {
            Transform(options.Transform);
            // Use Core's portable surface renderer, including its documented backdrop approximation.
            SurfaceEffects.Draw(this, in options);
            if (options.ClipToBounds && options.Bounds is { } bounds)
            {
                BeginPath(); RoundRect(bounds, options.CornerRadius); Clip(); BeginPath();
            }
            return new CompositionGroup(this, true);
        }
        catch
        {
            Restore();
            throw;
        }
    }
    public void EndGroup() => Restore();
    public void DrawImage(IImage image, Rect dest) => throw new InvalidOperationException("Unsupported Canvas operation: DrawImage(IImage image, Rect dest).");
    public void DrawImage(IImage image, Rect dest, NFloat opacity) => throw new InvalidOperationException("Unsupported Canvas operation: DrawImage(IImage image, Rect dest, NFloat opacity).");
    public void DrawImage(IImage image, Rect source, Rect dest, NFloat opacity) => throw new InvalidOperationException("Unsupported Canvas operation: DrawImage(IImage image, Rect source, Rect dest, NFloat opacity).");
    public void Translate(Vector value) => NativeCanvas.Command("Translate", (double)value.X, (double)value.Y);
    public void Rotate(NFloat value) => NativeCanvas.Command("Rotate", (double)value);
    public void Scale(Vector value) => NativeCanvas.Command("Scale", (double)value.X, (double)value.Y);
    public void SetTransform(AffineTransform value)
    {
        NativeCanvas.Command("TransformLinear", (double)value.A, (double)value.B, (double)value.C, (double)value.D);
        NativeCanvas.Command("SetTransformTranslation", (double)value.Tx, (double)value.Ty);
    }
    public void Transform(AffineTransform value)
    {
        NativeCanvas.Command("TransformLinear", (double)value.A, (double)value.B, (double)value.C, (double)value.D);
        NativeCanvas.Command("TransformTranslation", (double)value.Tx, (double)value.Ty);
    }
    public void MoveTo(Point value) => NativeCanvas.Command("MoveTo", (double)value.X, (double)value.Y);
    public void LineTo(Point value) => NativeCanvas.Command("LineTo", (double)value.X, (double)value.Y);
    public void CurveTo(Point control, Point to) => NativeCanvas.Command("QuadraticTo", (double)control.X, (double)control.Y, (double)to.X, (double)to.Y);
    public void CurveTo(Point control1, Point control2, Point to)
    {
        NativeCanvas.Command("CurveControls", (double)control1.X, (double)control1.Y, (double)control2.X, (double)control2.Y);
        NativeCanvas.Command("CubicTo", (double)to.X, (double)to.Y);
    }
    public void ClosePath() => NativeCanvas.Command("ClosePath");
    public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise)
    {
        NativeCanvas.Command("ArcOrigin", (double)center.X, (double)center.Y, (double)radius);
        NativeCanvas.Command("ArcAngles", (double)startAngle, (double)endAngle, (int)winding);
    }
    public void ArcTo(Point control1, Point control2, NFloat radius) => throw new InvalidOperationException("Unsupported Canvas operation: ArcTo(Point control1, Point control2, NFloat radius).");
    public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise)
    {
        NativeCanvas.Command("EllipseGeometry", (double)center.X, (double)center.Y, (double)radiusX, (double)radiusY);
        NativeCanvas.Command("EllipseAngles", (double)rotation, (double)startAngle, (double)endAngle, (int)winding);
    }
    public void Rect(Rect value) => NativeCanvas.Command("Rect", (double)value.X, (double)value.Y, (double)value.Width, (double)value.Height);
    public void RoundRect(Rect value, NFloat radius)
    {
        NativeCanvas.Command("PathRect", (double)value.X, (double)value.Y, (double)value.Width, (double)value.Height);
        NativeCanvas.Command("RoundRect", (double)radius);
    }
    public void RoundRect(Rect value, CornerRadius radius)
    {
        NativeCanvas.Command("PathRect", (double)value.X, (double)value.Y, (double)value.Width, (double)value.Height);
        NativeCanvas.Command("RoundRectCorners", (double)radius.TopLeft, (double)radius.TopRight, (double)radius.BottomRight, (double)radius.BottomLeft);
    }
    public void Fill(FillRule rule = FillRule.NonZero) => NativeCanvas.Command("Fill", (int)rule);
    public void Stroke() => NativeCanvas.Command("Stroke");
    public void Clip() => NativeCanvas.Command("Clip");
}

// Rewritten only at this explicit host boundary. The CLR body requires the browser host.
public static class NativeCanvas
{
    public static void Command(string name, double a = 0, double b = 0, double c = 0, double d = 0, string text = "") =>
        throw new InvalidOperationException("Browser Canvas import required.");
    public static double Measure(string text, int metric) => throw new InvalidOperationException("Browser text measurement import required.");
}
