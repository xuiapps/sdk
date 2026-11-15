using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Runtime.IOS.Actual;

public partial class IOSDrawingContext
{
    private readonly Path2D path2d = new Path2D(1024);

    /// <summary>
    /// Replays recorded path commands because CoreGraphics consumes its current path
    /// when it fills, strokes, or clips it.
    /// </summary>
    private void ReplayPath()
    {
        CGContextRef.CGContextBeginPath(this.cgContextRef);
        var sink = new CGReplaySink(this.cgContextRef);
        path2d.Visit(sink);
    }

    private struct CGReplaySink : IPathBuilder
    {
        private readonly nint context;

        public CGReplaySink(nint context) => this.context = context;

        public void BeginPath() { }
        public void MoveTo(Point to) => CGContextRef.CGContextMoveToPoint(context, to.X, to.Y);
        public void LineTo(Point to) => CGContextRef.CGContextAddLineToPoint(context, to.X, to.Y);
        public void ClosePath() => CGContextRef.CGContextClosePath(context);
        public void CurveTo(Point cp1, Point to) => CGContextRef.CGContextAddQuadCurveToPoint(context, cp1.X, cp1.Y, to.X, to.Y);
        public void CurveTo(Point cp1, Point cp2, Point to) => CGContextRef.CGContextAddCurveToPoint(context, cp1.X, cp1.Y, cp2.X, cp2.Y, to.X, to.Y);

        public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding) =>
            CGContextRef.CGContextAddArc(context, center.X, center.Y, radius, startAngle, endAngle, 1 - (int)winding);

        public void ArcTo(Point cp1, Point cp2, NFloat radius) =>
            CGContextRef.CGContextAddArcToPoint(context, cp1.X, cp1.Y, cp2.X, cp2.Y, radius);

        public void Rect(Rect rect) => CGContextRef.CGContextAddRect(context, rect);

        public void RoundRect(Rect rect, NFloat radius)
        {
            radius = RoundedRect.Clamp(rect.Size, radius);
            using var path = CGPathRef.CreateWithRoundedRect(rect, radius);
            CGContextRef.CGContextAddPath(context, path);
        }

        public void RoundRect(Rect rect, CornerRadius radius)
        {
            radius = RoundedRect.Clamp(rect.Size, radius);
            if (radius.TopLeft == 0) MoveTo(rect.TopLeft);
            else { MoveTo(new Point(rect.X, rect.Y + radius.TopLeft)); ArcTo(rect.TopLeft, rect.TopRight, radius.TopLeft); }
            if (radius.TopRight == 0) LineTo(rect.TopRight); else ArcTo(rect.TopRight, rect.BottomRight, radius.TopRight);
            if (radius.BottomRight == 0) LineTo(rect.BottomRight); else ArcTo(rect.BottomRight, rect.BottomLeft, radius.BottomRight);
            if (radius.BottomLeft == 0) LineTo(rect.BottomLeft); else ArcTo(rect.BottomLeft, rect.TopLeft, radius.BottomLeft);
            ClosePath();
        }

        public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding)
        {
            CGContextRef.CGContextSaveGState(context);
            CGContextRef.CGContextTranslateCTM(context, center.X, center.Y);
            CGContextRef.CGContextRotateCTM(context, rotation);
            CGContextRef.CGContextScaleCTM(context, radiusX, radiusY);
            CGContextRef.CGContextAddArc(context, 0, 0, 1, endAngle, startAngle, (int)winding);
            CGContextRef.CGContextRestoreGState(context);
        }
    }
}
