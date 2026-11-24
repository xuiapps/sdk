using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;

namespace TestAppCanvas;

// The first-frame host implements the state and rectangle operations used by View.
// Unsupported drawing operations throw instead of silently disappearing.
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
        if (segments.Length != 0) throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
        NativeCanvas.Command("SetLineDash");
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
    public TextMetrics MeasureText(string text) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void FillText(string text, Point pos) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void StrokeRect(Rect rect) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetStroke(LinearGradient value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetStroke(RadialGradient value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetStroke(ImagePattern value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetFill(LinearGradient value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetFill(RadialGradient value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetFill(ImagePattern value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetFilter() => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetFilter(Brightness value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public CompositionGroup Group(scoped in GroupOptions options) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void EndGroup() => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void DrawImage(IImage image, Rect dest) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void DrawImage(IImage image, Rect dest, NFloat opacity) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void DrawImage(IImage image, Rect source, Rect dest, NFloat opacity) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Translate(Vector value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Rotate(NFloat value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Scale(Vector value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void SetTransform(AffineTransform value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Transform(AffineTransform value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void MoveTo(Point value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void LineTo(Point value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void CurveTo(Point control, Point to) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void CurveTo(Point control1, Point control2, Point to) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void ClosePath() => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void ArcTo(Point control1, Point control2, NFloat radius) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding = Winding.ClockWise) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Rect(Rect value) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void RoundRect(Rect value, NFloat radius) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void RoundRect(Rect value, CornerRadius radius) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Fill(FillRule rule = FillRule.NonZero) => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Stroke() => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
    public void Clip() => throw new InvalidOperationException("Unsupported Canvas operation in the first-frame host.");
}

// Rewritten only at this explicit host boundary. The CLR body records the same calls.
public static class NativeCanvas
{
    public static readonly System.Collections.Generic.List<object[]> Commands = new();
    public static void Command(string name, double a = 0, double b = 0, double c = 0, double d = 0, string text = "") =>
        Commands.Add(new object[] { name, a, b, c, d, text });
}
