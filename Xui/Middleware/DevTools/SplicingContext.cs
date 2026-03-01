using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.DevTools;

/// <summary>
/// An <see cref="IContext"/> that forwards every draw call to two underlying contexts simultaneously:
/// a real platform context (for actual on-screen rendering) and a capture context (e.g. <see cref="Xui.Runtime.Software.Actual.SvgDrawingContext"/>).
/// This lets a single render pass produce both visible output and an SVG screenshot.
/// </summary>
internal sealed class SplicingContext(IContext real, IContext capture) : IContext
{
    // ── IStateContext ─────────────────────────────────────────────────────────
    public void Save()    { real.Save();    capture.Save(); }
    public void Restore() { real.Restore(); capture.Restore(); }

    // ── IPenContext ───────────────────────────────────────────────────────────
    nfloat IPenContext.GlobalAlpha    { set { real.GlobalAlpha    = value; capture.GlobalAlpha    = value; } }
    LineCap IPenContext.LineCap       { set { real.LineCap        = value; capture.LineCap        = value; } }
    LineJoin IPenContext.LineJoin     { set { real.LineJoin       = value; capture.LineJoin       = value; } }
    nfloat IPenContext.LineWidth      { set { real.LineWidth      = value; capture.LineWidth      = value; } }
    nfloat IPenContext.MiterLimit     { set { real.MiterLimit     = value; capture.MiterLimit     = value; } }
    nfloat IPenContext.LineDashOffset { set { real.LineDashOffset = value; capture.LineDashOffset = value; } }
    public void SetLineDash(ReadOnlySpan<nfloat> s) { real.SetLineDash(s); capture.SetLineDash(s); }
    public void SetFill(Color c)            { real.SetFill(c); capture.SetFill(c); }
    public void SetFill(LinearGradient g)   { real.SetFill(g); capture.SetFill(g); }
    public void SetFill(RadialGradient g)   { real.SetFill(g); capture.SetFill(g); }
    public void SetFill(ImagePattern p)     { real.SetFill(p); capture.SetFill(p); }
    public void SetStroke(Color c)          { real.SetStroke(c); capture.SetStroke(c); }
    public void SetStroke(LinearGradient g) { real.SetStroke(g); capture.SetStroke(g); }
    public void SetStroke(RadialGradient g) { real.SetStroke(g); capture.SetStroke(g); }
    public void SetStroke(ImagePattern p)   { real.SetStroke(p); capture.SetStroke(p); }

    // ── ITransformContext ─────────────────────────────────────────────────────
    public void Translate(Vector v)          { real.Translate(v);  capture.Translate(v); }
    public void Rotate(nfloat angle)         { real.Rotate(angle); capture.Rotate(angle); }
    public void Scale(Vector v)              { real.Scale(v);      capture.Scale(v); }
    public void SetTransform(AffineTransform t) { real.SetTransform(t); capture.SetTransform(t); }
    public void Transform(AffineTransform m)    { real.Transform(m);    capture.Transform(m); }

    // ── IRectDrawingContext ───────────────────────────────────────────────────
    public void FillRect(Rect r)   { real.FillRect(r);   capture.FillRect(r); }
    public void StrokeRect(Rect r) { real.StrokeRect(r); capture.StrokeRect(r); }

    // ── IPathBuilder / IGlyphPathBuilder ──────────────────────────────────────
    public void BeginPath()             { real.BeginPath();  capture.BeginPath(); }
    public void MoveTo(Point p)         { real.MoveTo(p);    capture.MoveTo(p); }
    public void LineTo(Point p)         { real.LineTo(p);    capture.LineTo(p); }
    public void ClosePath()             { real.ClosePath();  capture.ClosePath(); }
    public void CurveTo(Point cp, Point to)             { real.CurveTo(cp, to);           capture.CurveTo(cp, to); }
    public void CurveTo(Point cp1, Point cp2, Point to) { real.CurveTo(cp1, cp2, to);     capture.CurveTo(cp1, cp2, to); }
    public void ArcTo(Point cp1, Point cp2, nfloat r)   { real.ArcTo(cp1, cp2, r);        capture.ArcTo(cp1, cp2, r); }
    public void Arc(Point c, nfloat r, nfloat s, nfloat e, Winding w = Winding.ClockWise) { real.Arc(c,r,s,e,w); capture.Arc(c,r,s,e,w); }
    public void Ellipse(Point c, nfloat rx, nfloat ry, nfloat rot, nfloat s, nfloat e, Winding w = Winding.ClockWise) { real.Ellipse(c,rx,ry,rot,s,e,w); capture.Ellipse(c,rx,ry,rot,s,e,w); }
    public void Rect(Rect r)                        { real.Rect(r);           capture.Rect(r); }
    public void RoundRect(Rect r, nfloat radius)    { real.RoundRect(r,radius); capture.RoundRect(r,radius); }
    public void RoundRect(Rect r, CornerRadius cr)  { real.RoundRect(r,cr);    capture.RoundRect(r,cr); }

    // ── IPathDrawing ──────────────────────────────────────────────────────────
    public void Fill(FillRule rule = FillRule.NonZero) { real.Fill(rule); capture.Fill(rule); }
    public void Stroke()                               { real.Stroke();   capture.Stroke(); }

    // ── IPathClipping ─────────────────────────────────────────────────────────
    public void Clip() { real.Clip(); capture.Clip(); }

    // ── ITextDrawingContext ───────────────────────────────────────────────────
    TextAlign    ITextDrawingContext.TextAlign    { set { real.TextAlign    = value; capture.TextAlign    = value; } }
    TextBaseline ITextDrawingContext.TextBaseline { set { real.TextBaseline = value; capture.TextBaseline = value; } }
    public void FillText(string text, Point pos)         { real.FillText(text, pos); capture.FillText(text, pos); }
    public void FillText(ReadOnlySpan<char> text, Point pos) { real.FillText(text, pos); capture.FillText(text, pos); }

    // ── ITextMeasureContext / IMeasureContext ─────────────────────────────────
    // Delegate measurement to the real context (accurate platform metrics).
    public void SetFont(Font font)               { real.SetFont(font); capture.SetFont(font); }
    public TextMetrics MeasureText(string text)  => real.MeasureText(text);
    public TextMetrics MeasureText(ReadOnlySpan<char> text) => real.MeasureText(text);

    // ── IImageDrawingContext ──────────────────────────────────────────────────
    public void DrawImage(IImage image, Rect dest)                       { real.DrawImage(image, dest);             capture.DrawImage(image, dest); }
    public void DrawImage(IImage image, Rect dest, nfloat opacity)       { real.DrawImage(image, dest, opacity);    capture.DrawImage(image, dest, opacity); }
    public void DrawImage(IImage image, Rect src, Rect dest, nfloat op)  { real.DrawImage(image, src, dest, op);   capture.DrawImage(image, src, dest, op); }

    // ── IDisposable ───────────────────────────────────────────────────────────
    /// <summary>
    /// Disposes both the real and capture contexts.
    /// Disposing the capture context (<see cref="Xui.Runtime.Software.Actual.SvgDrawingContext"/>)
    /// writes the closing SVG tags, making the stream ready to read.
    /// </summary>
    public void Dispose() { real.Dispose(); capture.Dispose(); }
}
