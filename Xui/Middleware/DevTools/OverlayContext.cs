using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.DevTools;

/// <summary>
/// An <see cref="IContext"/> wrapper that draws pointer interaction overlays (mouse cursor arrow
/// or touch circle) on top of the frame content before disposing the underlying context.
/// This makes AI-driven interactions visible in both the live window and SVG screenshots.
/// </summary>
internal sealed class OverlayContext(IContext inner, Point pos, bool isTouch, string? label = null) : IContext
{
    // ── IStateContext ─────────────────────────────────────────────────────────
    public void Save()    => inner.Save();
    public void Restore() => inner.Restore();

    // ── IPenContext ───────────────────────────────────────────────────────────
    nfloat IPenContext.GlobalAlpha    { set => inner.GlobalAlpha    = value; }
    LineCap IPenContext.LineCap       { set => inner.LineCap        = value; }
    LineJoin IPenContext.LineJoin     { set => inner.LineJoin       = value; }
    nfloat IPenContext.LineWidth      { set => inner.LineWidth      = value; }
    nfloat IPenContext.MiterLimit     { set => inner.MiterLimit     = value; }
    nfloat IPenContext.LineDashOffset { set => inner.LineDashOffset = value; }
    public void SetLineDash(ReadOnlySpan<nfloat> s) => inner.SetLineDash(s);
    public void SetFill(Color c)            => inner.SetFill(c);
    public void SetFill(LinearGradient g)   => inner.SetFill(g);
    public void SetFill(RadialGradient g)   => inner.SetFill(g);
    public void SetFill(ImagePattern p)     => inner.SetFill(p);
    public void SetStroke(Color c)          => inner.SetStroke(c);
    public void SetStroke(LinearGradient g) => inner.SetStroke(g);
    public void SetStroke(RadialGradient g) => inner.SetStroke(g);
    public void SetStroke(ImagePattern p)   => inner.SetStroke(p);

    // ── ITransformContext ─────────────────────────────────────────────────────
    public void Translate(Vector v)             => inner.Translate(v);
    public void Rotate(nfloat angle)            => inner.Rotate(angle);
    public void Scale(Vector v)                 => inner.Scale(v);
    public void SetTransform(AffineTransform t) => inner.SetTransform(t);
    public void Transform(AffineTransform m)    => inner.Transform(m);

    // ── IRectDrawingContext ───────────────────────────────────────────────────
    public void FillRect(Rect r)   => inner.FillRect(r);
    public void StrokeRect(Rect r) => inner.StrokeRect(r);

    // ── IPathBuilder ──────────────────────────────────────────────────────────
    public void BeginPath()                                                    => inner.BeginPath();
    public void MoveTo(Point p)                                                => inner.MoveTo(p);
    public void LineTo(Point p)                                                => inner.LineTo(p);
    public void ClosePath()                                                    => inner.ClosePath();
    public void CurveTo(Point cp, Point to)                                    => inner.CurveTo(cp, to);
    public void CurveTo(Point cp1, Point cp2, Point to)                        => inner.CurveTo(cp1, cp2, to);
    public void ArcTo(Point cp1, Point cp2, nfloat r)                          => inner.ArcTo(cp1, cp2, r);
    public void Arc(Point c, nfloat r, nfloat s, nfloat e, Winding w = Winding.ClockWise)                                => inner.Arc(c, r, s, e, w);
    public void Ellipse(Point c, nfloat rx, nfloat ry, nfloat rot, nfloat s, nfloat e, Winding w = Winding.ClockWise)   => inner.Ellipse(c, rx, ry, rot, s, e, w);
    public void Rect(Rect r)                       => inner.Rect(r);
    public void RoundRect(Rect r, nfloat radius)   => inner.RoundRect(r, radius);
    public void RoundRect(Rect r, CornerRadius cr) => inner.RoundRect(r, cr);

    // ── IPathDrawing ──────────────────────────────────────────────────────────
    public void Fill(FillRule rule = FillRule.NonZero) => inner.Fill(rule);
    public void Stroke()                               => inner.Stroke();

    // ── IPathClipping ─────────────────────────────────────────────────────────
    public void Clip() => inner.Clip();

    // ── ITextDrawingContext ───────────────────────────────────────────────────
    TextAlign    ITextDrawingContext.TextAlign    { set => inner.TextAlign    = value; }
    TextBaseline ITextDrawingContext.TextBaseline { set => inner.TextBaseline = value; }
    public void FillText(string text, Point p)             => inner.FillText(text, p);
    public void FillText(ReadOnlySpan<char> text, Point p) => inner.FillText(text, p);

    // ── IMeasureContext ───────────────────────────────────────────────────────
    public void SetFont(Font font)                          => inner.SetFont(font);
    public TextMetrics MeasureText(string text)             => inner.MeasureText(text);
    public TextMetrics MeasureText(ReadOnlySpan<char> text) => inner.MeasureText(text);

    // ── IImageDrawingContext ──────────────────────────────────────────────────
    public void DrawImage(IImage image, Rect dest)                      => inner.DrawImage(image, dest);
    public void DrawImage(IImage image, Rect dest, nfloat opacity)      => inner.DrawImage(image, dest, opacity);
    public void DrawImage(IImage image, Rect src, Rect dest, nfloat op) => inner.DrawImage(image, src, dest, op);

    // ── IDisposable ───────────────────────────────────────────────────────────
    /// <summary>
    /// Draws the pointer overlay on top of the frame content, then disposes the underlying context.
    /// Drawing here (just before Dispose) ensures the overlay appears above all app content,
    /// and is captured in both the real window and any active SVG screenshot stream.
    /// </summary>
    public void Dispose()
    {
        inner.Save();
        if (isTouch)
            DrawTouchIndicator();
        else
            DrawMouseCursor();
        inner.Restore();
        inner.Dispose();
    }

    // ── Overlay drawing ───────────────────────────────────────────────────────

    private void DrawTouchIndicator()
    {
        inner.BeginPath();
        inner.Ellipse(pos, 15f, 15f, 0, 0, (nfloat)(System.Math.PI * 2), Winding.ClockWise);
        inner.SetFill(0x66888888);
        inner.Fill();

        inner.BeginPath();
        inner.Ellipse(pos, 15f, 15f, 0, 0, (nfloat)(System.Math.PI * 2), Winding.ClockWise);
        inner.LineWidth = 3f;
        inner.SetStroke(0x88AAAAAA);
        inner.Stroke();

        DrawLabel(new Point(pos.X + 18, pos.Y - 6));
    }

    private void DrawMouseCursor()
    {
        // Arrow cursor polygon matching the TestSinglePageApp snapshot cursor shape.
        // Drawn at absolute coordinates — no Translate needed.
        var x = pos.X;
        var y = pos.Y;
        inner.BeginPath();
        inner.MoveTo(new Point(x,     y));
        inner.LineTo(new Point(x,     y + 12));
        inner.LineTo(new Point(x + 3, y + 9));
        inner.LineTo(new Point(x + 5, y + 12));
        inner.LineTo(new Point(x + 7, y + 11));
        inner.LineTo(new Point(x + 5, y + 8));
        inner.LineTo(new Point(x + 9, y + 8));
        inner.ClosePath();
        inner.SetFill(0xFFFFFFFF);
        inner.Fill();
        inner.LineWidth = (nfloat)0.7;
        inner.SetStroke(0x000000FF);
        inner.Stroke();

        DrawLabel(new Point(x + 12, y + 14));
    }

    private void DrawLabel(Point anchor)
    {
        if (label == null) return;

        inner.SetFont(new Font(11, ["Inter"]));
        var m = inner.MeasureText(label);
        var pad = (nfloat)3;
        var w = (nfloat)m.Size.Width + pad * 2;
        var h = (nfloat)m.Size.Height + pad * 2;

        // Semi-transparent dark pill background.
        inner.SetFill(0x000000AA);
        inner.BeginPath();
        inner.RoundRect(new Rect(anchor.X, anchor.Y, w, h), (nfloat)4);
        inner.Fill();

        // White label text.
        inner.TextBaseline = TextBaseline.Top;
        inner.TextAlign    = TextAlign.Left;
        inner.SetFill(0xFFFFFFFF);
        inner.FillText(label, new Point(anchor.X + pad, anchor.Y + pad));
    }
}
