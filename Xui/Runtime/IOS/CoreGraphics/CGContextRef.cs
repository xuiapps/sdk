using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    public ref partial struct CGContextRef
    {
        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextRelease(nint self);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetRGBFillColor(nint cgContextRef, NFloat red, NFloat green, NFloat blue, NFloat alpha);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetRGBStrokeColor(nint cgContextRef, NFloat red, NFloat green, NFloat blue, NFloat alpha);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetLineWidth(nint cgContextRef, NFloat width);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextFillRect(nint cgContextRef, CGRect rect);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextStrokeRect(nint cgContextRef, CGRect rect);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddPath(nint cgContextRef, nint cgPathRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextMoveToPoint(nint cgContextRef, NFloat x, NFloat y);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddLineToPoint(nint cgContextRef, NFloat x, NFloat y);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddRect(nint cgContextRef, CGRect cgRect);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddArc(nint cgContextRef, NFloat x, NFloat y, NFloat radius, NFloat startAngle, NFloat endAngle, int clockwise);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddArcToPoint(nint cgContextRef, NFloat x1, NFloat y1, NFloat x2, NFloat y2, NFloat radius);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddQuadCurveToPoint(nint c, NFloat cpx, NFloat cpy, NFloat x, NFloat y);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextAddCurveToPoint(nint c, NFloat cp1x, NFloat cp1y, NFloat cp2x, NFloat cp2y, NFloat x, NFloat y);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextBeginPath(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextClosePath(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextClip(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextClipToRect(nint c, CGRect rect);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextEOClip(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetAlpha(nint cgContextRef, NFloat alpha);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetLineCap(nint cgContextRef, CGLineCap cap);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetLineJoin(nint cgContextRef, CGLineJoin join);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetMiterLimit(nint cgContextRef, NFloat limit);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextFillPath(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextEOFillPath(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextStrokePath(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextDrawLinearGradient(nint cgContextRef, nint cgGradientRef, CGPoint startPoint, CGPoint endPoint, uint options = 3);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextDrawRadialGradient(nint cgContextRef, nint cgGradientRef, CGPoint startCenter, NFloat startRadius, CGPoint endCenter, NFloat endRadius, uint options = 3);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextDrawImage(nint cgContextRef, CGRect rect, nint image);

        [LibraryImport(CoreGraphicsLib)]
        public static partial CGRect CGContextGetClipBoundingBox(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextReplacePathWithStrokedPath(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSaveGState(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextRestoreGState(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextConcatCTM(nint cgContextRef, CGAffineTransform transform);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetLineDash(nint c, NFloat phase, ref NFloat cgFloatArrPtr, nint count);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetLineDash(nint c, NFloat phase, nint cgFloatArrPtr, nint count);

        [LibraryImport(CoreGraphicsLib)]
        public static partial CGAffineTransform CGContextGetCTM(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextRotateCTM(nint cgContextRef, NFloat angle);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextScaleCTM(nint cgContextRef, NFloat sx, NFloat sy);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextTranslateCTM(nint cgContextRef, NFloat tx, NFloat ty);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetTextDrawingMode(nint cgContextRef, CGTextDrawingMode mode);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextBeginTransparencyLayer(nint cgContextRef, nint auxiliaryInfo);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextEndTransparencyLayer(nint cgContextRef);

        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGContextSetBlendMode(nint cgContextRef, CGBlendMode blendMode);

        [LibraryImport(CoreGraphicsLib)]
        public static partial nint UIGraphicsGetCurrentContext();

        public readonly nint Self;

        public CGContextRef(nint self)
        {
            if (self == 0)
            {
                throw new ObjCException($"{nameof(CGContextRef)} instantiated with nil self.");
            }

            this.Self = self;
        }

        public CGAffineTransform Transform
        {
            get => CGContextGetCTM(this);
        }

        public NFloat Alpha
        {
            set => CGContextSetAlpha(this, value);
        }

        public CGLineCap LineCap
        {
            set => CGContextSetLineCap(this, value);
        }

        public CGLineJoin LineJoin
        {
            set => CGContextSetLineJoin(this, value);
        }

        public CGAffineTransform CTM =>
            CGContextGetCTM(this);

        public void SetRGBFillColor(NFloat red, NFloat green, NFloat blue, NFloat alpha) =>
            CGContextSetRGBFillColor(this, red, green, blue, alpha);

        public void SetRGBStrokeColor(NFloat red, NFloat green, NFloat blue, NFloat alpha) =>
            CGContextSetRGBStrokeColor(this, red, green, blue, alpha);
        
        public void SetLineWidth(NFloat width) =>
            CGContextSetLineWidth(this, width);

        public void SetMiterLimit(NFloat limit) =>
            CGContextSetMiterLimit(this, limit);
        
        public void FillRect(CGRect rect) =>
            CGContextFillRect(this, rect);

        public void StrokeRect(CGRect rect) =>
            CGContextStrokeRect(this, rect);
        
        public void AddPath(CGPathRef path) =>
            CGContextAddPath(this, path);

        public void AddRect(CGRect rect) =>
            CGContextAddRect(this, rect);
        
        public void MoveToPoint(NFloat x, NFloat y) =>
            CGContextMoveToPoint(this, x, y);

        public void LineToPoint(NFloat x, NFloat y) =>
            CGContextAddLineToPoint(this, x, y);
        
        public void AddArc(NFloat x, NFloat y, NFloat radius, NFloat startAngle, NFloat endAngle, bool clockwise) =>
            CGContextAddArc(this, x, y, radius, startAngle, endAngle, clockwise ? 1 : 0);

        public void AddArcToPoint(NFloat x1, NFloat y1, NFloat x2, NFloat y2, NFloat radius) =>
            CGContextAddArcToPoint(this, x1, y1, x2, y2, radius);
        
        public void CGContextAddQuadCurveToPoint(NFloat cpx, NFloat cpy, NFloat x, NFloat y) =>
            CGContextAddQuadCurveToPoint(this, cpx, cpy, x, y);

        public void CGContextAddCurveToPoint(NFloat cp1x, NFloat cp1y, NFloat cp2x, NFloat cp2y, NFloat x, NFloat y) =>
            CGContextAddCurveToPoint(this, cp1x, cp1y, cp2x, cp2y, x, y);

        public void BeginPath() =>
            CGContextBeginPath(this);

        public void ClosePath() =>
            CGContextClosePath(this);

        public void Clip() =>
            CGContextClip(this);

        public void EOClip() =>
            CGContextEOClip(this);

        public void EOFillPath() =>
            CGContextEOFillPath(this);

        public void FillPath() =>
            CGContextFillPath(this);

        public void StrokePath() =>
            CGContextStrokePath(this);
        
        public void DrawLinearGradient(CGGradientRef gradient, CGPoint startPoint, CGPoint endPoint) =>
            CGContextDrawLinearGradient(this, gradient, startPoint, endPoint);
        
        public void DrawRadialGradient(CGGradientRef gradient, CGPoint startCenter, NFloat startRadius, CGPoint endCenter, NFloat endRadius) =>
            CGContextDrawRadialGradient(this, gradient, startCenter, startRadius, endCenter, endRadius);

        public void SaveGState() =>
            CGContextSaveGState(this);
        public void RestoreGState() =>
            CGContextRestoreGState(this);
        
        public void ReplacePathWithStrokedPath() =>
            CGContextReplacePathWithStrokedPath(this);

        public void ConcatCTM(CGAffineTransform transform) =>
            CGContextConcatCTM(this, transform);

        public void RotateCTM(NFloat angle) =>
            CGContextRotateCTM(this, angle);

        public void ScaleCTM(NFloat sx, NFloat sy) =>
            CGContextScaleCTM(this, sx, sy);

        public void TranslateCTM(NFloat tx, NFloat ty) =>
            CGContextTranslateCTM(this, tx, ty);

        public static implicit operator nint(CGContextRef path) => path.Self;

        public void Dispose()
        {
            if (this.Self != 0)
            {
                CGContextRelease(this.Self);
            }
        }
    }
}
