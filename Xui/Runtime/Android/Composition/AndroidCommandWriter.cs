using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;
using Xui.Runtime.Android.Actual;


namespace Xui.Runtime.Android.Composition;

// Replays the shared command buffer into a platform wire stream only when content changes.
internal sealed class AndroidCommandWriter(WireBuffer buffer, IAndroidResources resources) : IContext
{
    private void Op(DrawOp op) => buffer.Int((int)op);
    public void Dispose() { }
    public CompositionGroup Group(scoped in GroupOptions options) => throw new InvalidOperationException("Groups are reconciled before transport.");
    public void EndGroup() => throw new InvalidOperationException("Groups are reconciled before transport.");
    public TextMetrics MeasureText(string text) => throw new InvalidOperationException("Use the window measurement service.");
    public void Save() => Op(DrawOp.Save);
    public void Restore() => Op(DrawOp.Restore);
    public void BeginPath() => Op(DrawOp.BeginPath);
    public void ClosePath() => Op(DrawOp.ClosePath);
    public void Stroke() => Op(DrawOp.Stroke);
    public void Clip() => Op(DrawOp.Clip);
    public NFloat GlobalAlpha { set { Op(DrawOp.GlobalAlpha); buffer.Float(value); } }
    public NFloat LineWidth { set { Op(DrawOp.LineWidth); buffer.Float(value); } }
    public NFloat MiterLimit { set { Op(DrawOp.MiterLimit); buffer.Float(value); } }
    public NFloat LineDashOffset { set { Op(DrawOp.LineDashOffset); buffer.Float(value); } }
    public LineCap LineCap { set { Op(DrawOp.LineCap); buffer.Int((int)value); } }
    public LineJoin LineJoin { set { Op(DrawOp.LineJoin); buffer.Int((int)value); } }
    public TextAlign TextAlign { set { Op(DrawOp.TextAlign); buffer.Int((int)value); } }
    public TextBaseline TextBaseline { set { Op(DrawOp.TextBaseline); buffer.Int((int)value); } }
    public void Translate(Vector v) { Op(DrawOp.Translate); buffer.Vector(v); }
    public void Scale(Vector v) { Op(DrawOp.Scale); buffer.Vector(v); }
    public void Transform(AffineTransform m) { Op(DrawOp.Transform); buffer.Matrix(m); }
    public void SetTransform(AffineTransform m) { Op(DrawOp.SetTransform); buffer.Matrix(m); }
    public void FillRect(Rect r) { Op(DrawOp.FillRect); buffer.Rect(r); }
    public void StrokeRect(Rect r) { Op(DrawOp.StrokeRect); buffer.Rect(r); }
    public void Rect(Rect r) { Op(DrawOp.Rect); buffer.Rect(r); }
    public void MoveTo(Point p) { Op(DrawOp.MoveTo); buffer.Point(p); }
    public void LineTo(Point p) { Op(DrawOp.LineTo); buffer.Point(p); }
    public void Rotate(NFloat angle) { Op(DrawOp.Rotate); buffer.Float(angle); }
    public void CurveTo(Point control, Point end) { Op(DrawOp.Quadratic); buffer.Point(control); buffer.Point(end); }
    public void CurveTo(Point a, Point b, Point end) { Op(DrawOp.Cubic); buffer.Point(a); buffer.Point(b); buffer.Point(end); }
    public void ArcTo(Point a, Point b, NFloat radius) { Op(DrawOp.ArcTo); buffer.Point(a); buffer.Point(b); buffer.Float(radius); }
    public void Arc(Point center, NFloat radius, NFloat start, NFloat end, Winding winding = Winding.ClockWise) => Ellipse(center, radius, radius, 0, start, end, winding);
    public void Ellipse(Point center, NFloat rx, NFloat ry, NFloat rotation, NFloat start, NFloat end, Winding winding = Winding.ClockWise)
    { Op(DrawOp.Ellipse); buffer.Point(center); buffer.Float(rx); buffer.Float(ry); buffer.Float(rotation); buffer.Float(start); buffer.Float(end); buffer.Int((int)winding); }
    public void RoundRect(Rect rect, NFloat radius) => RoundRect(rect, new CornerRadius(radius));
    public void RoundRect(Rect rect, CornerRadius radius)
    { Op(DrawOp.RoundRect); buffer.Rect(rect); buffer.Float(radius.TopLeft); buffer.Float(radius.TopRight); buffer.Float(radius.BottomRight); buffer.Float(radius.BottomLeft); }
    public void Fill(FillRule rule = FillRule.NonZero) { Op(DrawOp.Fill); buffer.Int((int)rule); }
    public void SetFilter() => Op(DrawOp.FilterNone);
    public void SetFilter(Brightness brightness) { Op(DrawOp.Brightness); buffer.Float(brightness.Amount); }
    public void SetFont(Font font)
    { Op(DrawOp.Font); buffer.Int(resources.Font(font)); buffer.Float(font.FontSize); buffer.Float(font.FontStretch.Value / 100); buffer.Float(font.FontStyle.IsOblique ? font.FontStyle.ObliqueAngle : 0); }
    public void SetFill(Color color) { Op(DrawOp.FillColor); buffer.Color(color); }
    public void SetStroke(Color color) { Op(DrawOp.StrokeColor); buffer.Color(color); }
    public void SetFill(LinearGradient g) { Op(DrawOp.FillLinear); Linear(g); }
    public void SetStroke(LinearGradient g) { Op(DrawOp.StrokeLinear); Linear(g); }
    public void SetFill(RadialGradient g) { Op(DrawOp.FillRadial); Radial(g); }
    public void SetStroke(RadialGradient g) { Op(DrawOp.StrokeRadial); Radial(g); }
    private void Linear(LinearGradient g) { buffer.Point(g.StartPoint); buffer.Point(g.EndPoint); Stops(g.GradientStops); }
    private void Radial(RadialGradient g) { buffer.Point(g.StartCenter); buffer.Float(g.StartRadius); buffer.Point(g.EndCenter); buffer.Float(g.EndRadius); Stops(g.GradientStops); }
    private void Stops(ReadOnlySpan<GradientStop> stops) { buffer.Int(stops.Length); foreach (var stop in stops) { buffer.Float(stop.Offset); buffer.Color(stop.Color); } }
    public void SetFill(ImagePattern p) { Op(DrawOp.FillPattern); buffer.Int(resources.Image(p.Image)); buffer.Int((int)p.Repetition); }
    public void SetStroke(ImagePattern p) { Op(DrawOp.StrokePattern); buffer.Int(resources.Image(p.Image)); buffer.Int((int)p.Repetition); }
    public void FillText(string text, Point position) => FillText(text.AsSpan(), position);
    public void FillText(ReadOnlySpan<char> text, Point position) { Op(DrawOp.Text); buffer.Point(position); buffer.Text(text); }
    public void SetLineDash(ReadOnlySpan<NFloat> segments) { Op(DrawOp.Dashes); buffer.Int(segments.Length); foreach (var segment in segments) buffer.Float(segment); }
    public void DrawImage(IImage image, Rect dest) => DrawImage(image, dest, 1);
    public void DrawImage(IImage image, Rect dest, NFloat opacity) { Op(DrawOp.Image); buffer.Int(resources.Image(image)); buffer.Rect(dest); buffer.Float(opacity); }
    public void DrawImage(IImage image, Rect source, Rect dest, NFloat opacity) { Op(DrawOp.ImageCrop); buffer.Int(resources.Image(image)); buffer.Rect(source); buffer.Rect(dest); buffer.Float(opacity); }
}
