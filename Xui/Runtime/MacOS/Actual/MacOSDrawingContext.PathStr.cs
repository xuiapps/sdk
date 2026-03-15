using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using static Xui.Runtime.MacOS.CoreGraphics;

namespace Xui.Runtime.MacOS.Actual;

public partial class MacOSDrawingContext
{
    private readonly Path2D path2d = new Path2D(1024);

    /// <summary>
    /// Replays the recorded path commands into the CG context so that Fill, Stroke,
    /// or Clip can use a fresh copy of the path (CG fill/stroke ops consume the path).
    /// </summary>
    private void ReplayPath()
    {
        CGContextRef.CGContextBeginPath(this.cgContextRef);
        var sink = new CGReplaySink(this.cgContextRef);
        path2d.Visit(sink);
    }

    /// <summary>
    /// Forwards recorded path commands directly to the CoreGraphics context.
    /// BeginPath is a no-op here — <see cref="ReplayPath"/> already calls CGContextBeginPath.
    /// </summary>
    private struct CGReplaySink : IPathBuilder
    {
        private readonly nint ctx;

        public CGReplaySink(nint ctx) => this.ctx = ctx;

        public void BeginPath() { } // handled by ReplayPath

        public void MoveTo(Point to) =>
            CGContextRef.CGContextMoveToPoint(ctx, to.X, to.Y);

        public void LineTo(Point to) =>
            CGContextRef.CGContextAddLineToPoint(ctx, to.X, to.Y);

        public void ClosePath() =>
            CGContextRef.CGContextClosePath(ctx);

        public void CurveTo(Point cp1, Point to) =>
            CGContextRef.CGContextAddQuadCurveToPoint(ctx, cp1.X, cp1.Y, to.X, to.Y);

        public void CurveTo(Point cp1, Point cp2, Point to) =>
            CGContextRef.CGContextAddCurveToPoint(ctx, cp1.X, cp1.Y, cp2.X, cp2.Y, to.X, to.Y);

        public void Arc(Point c, NFloat r, NFloat s, NFloat e, Winding w) =>
            CGContextRef.CGContextAddArc(ctx, c.X, c.Y, r, s, e, 1 - (int)w);

        public void ArcTo(Point cp1, Point cp2, NFloat r) =>
            CGContextRef.CGContextAddArcToPoint(ctx, cp1.X, cp1.Y, cp2.X, cp2.Y, r);

        public void Rect(Rect rect) =>
            CGContextRef.CGContextAddRect(ctx, rect);

        public void RoundRect(Rect rect, NFloat radius)
        {
            using var cgPath = CGPathRef.CreateWithRoundedRect(rect, radius);
            CGContextRef.CGContextAddPath(ctx, cgPath);
        }

        public void RoundRect(Rect rect, CornerRadius radius)
        {
            if (radius.TopLeft == 0)
                MoveTo(rect.TopLeft);
            else
            {
                MoveTo(new Point(rect.X, rect.Y + radius.TopLeft));
                ArcTo(rect.TopLeft, rect.TopRight, radius.TopLeft);
            }

            if (radius.TopRight == 0)
                LineTo(rect.TopRight);
            else
                ArcTo(rect.TopRight, rect.BottomRight, radius.TopRight);

            if (radius.BottomRight == 0)
                LineTo(rect.BottomRight);
            else
                ArcTo(rect.BottomRight, rect.BottomLeft, radius.BottomRight);

            if (radius.BottomLeft == 0)
                LineTo(rect.BottomLeft);
            else
                ArcTo(rect.BottomLeft, rect.TopLeft, radius.BottomLeft);

            ClosePath();
        }

        public void Ellipse(Point center, NFloat rx, NFloat ry, NFloat rotation, NFloat s, NFloat e, Winding w)
        {
            // Apply center+rotation+scale as a CTM modification so CGContextAddArc
            // draws an ellipse. The current path is NOT part of CG graphics state, so
            // Save/Restore here only affects the transform — the arc segments survive.
            CGContextRef.CGContextSaveGState(ctx);
            CGContextRef.CGContextTranslateCTM(ctx, center.X, center.Y);
            CGContextRef.CGContextRotateCTM(ctx, rotation);
            CGContextRef.CGContextScaleCTM(ctx, rx, ry);
            CGContextRef.CGContextAddArc(ctx, 0, 0, 1, e, s, (int)w);
            CGContextRef.CGContextRestoreGState(ctx);
        }
    }
}
