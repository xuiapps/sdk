using System.Runtime.InteropServices;
using static Xui.Runtime.MacOS.AppKit;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.CoreGraphics;
using static Xui.Runtime.MacOS.Foundation;
using static Xui.Runtime.MacOS.CoreText;
using System.Diagnostics;
using System;
using Xui.Core.Math2D;
using Xui.Core.Canvas;

namespace Xui.Runtime.MacOS.Actual;

public partial class MacOSDrawingContext : IContext
{
    private nint cgContextRef;

    private Paint fill;

    private Paint stroke;

    internal readonly MacOSTextMeasureContext textMeasure = new();

    private AffineTransform? baseTransform;

    private NFloat lineDashOffset = 0f;

    public MacOSDrawingContext()
    {
    }

    NFloat IPenContext.GlobalAlpha { set => CGContextRef.CGContextSetAlpha(this.cgContextRef, value); }

    LineCap IPenContext.LineCap { set => CGContextRef.CGContextSetLineCap(this.cgContextRef, (CGLineCap)value); }
    LineJoin IPenContext.LineJoin { set => CGContextRef.CGContextSetLineJoin(this.cgContextRef, (CGLineJoin)value); }
    NFloat IPenContext.LineWidth { set => CGContextRef.CGContextSetLineWidth(this.cgContextRef, value); }
    NFloat IPenContext.MiterLimit
    {
        set
        {
            if (this.lineDashOffset != value)
            {
                this.lineDashOffset = value;
                // TODO: Re-do the CGContextRef.CGContextSetLineDash calls and apply the offset
            }
        }
    }

    public NFloat LineDashOffset { get; set; }

    public TextAlign TextAlign { get => textMeasure.TextAlign; set => textMeasure.TextAlign = value; }

    public TextBaseline TextBaseline { get; set; }

    public MacOSDrawingContext Bind()
    {
        this.cgContextRef = NSGraphicsContext.CurrentCGContext.Self;
        this.fill.Reset();
        this.stroke.Reset();
        this.baseTransform = null;
        this.path2d.BeginPath();

        return this;
    }

    void IStateContext.Restore() =>
        CGContextRef.CGContextRestoreGState(this.cgContextRef);

    void IStateContext.Save() =>
        CGContextRef.CGContextSaveGState(this.cgContextRef);

    void IPenContext.SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        // TODO: Offset the segments with LineDashOffset...

        if (segments.Length == 0)
        {
            CGContextRef.CGContextSetLineDash(this.cgContextRef, this.LineDashOffset, nint.Zero, nint.Zero);
        }
        else if (segments.Length % 2 == 0)
        {
            CGContextRef.CGContextSetLineDash(this.cgContextRef, this.LineDashOffset, ref MemoryMarshal.GetReference(segments), segments.Length);
        }
        else
        {
            Span<NFloat> mirrored = stackalloc NFloat[segments.Length * 2];
            for(var i = 0; i < segments.Length; i++)
            {
                mirrored[i] = mirrored[i * 2] = segments[i];
            }
            CGContextRef.CGContextSetLineDash(this.cgContextRef, this.LineDashOffset, ref MemoryMarshal.GetReference(mirrored), mirrored.Length);
        }
    }

    void IPathBuilder.Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding) =>
        path2d.Arc(center, radius, startAngle, endAngle, winding);

    void IPathBuilder.ArcTo(Point cp1, Point cp2, NFloat radius) =>
        path2d.ArcTo(cp1, cp2, radius);

    void IPathBuilder.BeginPath() =>
        path2d.BeginPath();

    void IPathClipping.Clip()
    {
        ReplayPath();
        CGContextRef.CGContextClip(this.cgContextRef);
    }

    void IGlyphPathBuilder.ClosePath() =>
        path2d.ClosePath();

    void IGlyphPathBuilder.CurveTo(Point cp1, Point to) =>
        path2d.CurveTo(cp1, to);

    void IPathBuilder.CurveTo(Point cp1, Point cp2, Point to) =>
        path2d.CurveTo(cp1, cp2, to);

    void IDisposable.Dispose()
    {
        if (this.cgContextRef != nint.Zero)
        {
            this.cgContextRef = nint.Zero;
        }

        this.fill.Reset();
        this.stroke.Reset();

        this.baseTransform = null;

        textMeasure.Dispose();
    }

    void IPathBuilder.Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding) =>
        path2d.Ellipse(center, radiusX, radiusY, rotation, startAngle, endAngle, winding);

    void IPathDrawing.Fill(FillRule fillRule)
    {
        ReplayPath();

        if (this.fill.style == PaintStyle.SolidColor)
        {
            if (fillRule == FillRule.EvenOdd)
            {
                CGContextRef.CGContextEOFillPath(this.cgContextRef);
            }
            else
            {
                CGContextRef.CGContextFillPath(this.cgContextRef);
            }
        }
        else if (this.fill.style == PaintStyle.LinearGradient)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            if (fillRule == FillRule.EvenOdd)
            {
                CGContextRef.CGContextEOClip(this.cgContextRef);
            }
            else
            {
                CGContextRef.CGContextClip(this.cgContextRef);
            }

            CGContextRef.CGContextDrawLinearGradient(
                this.cgContextRef,
                this.fill.cgGradientRef,
                this.fill.startPoint,
                this.fill.endPoint);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else if (this.fill.style == PaintStyle.BitmapBrush)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            if (fillRule == FillRule.EvenOdd)
                CGContextRef.CGContextEOClip(this.cgContextRef);
            else
                CGContextRef.CGContextClip(this.cgContextRef);
            TileImagePattern(this.fill);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            if (fillRule == FillRule.EvenOdd)
            {
                CGContextRef.CGContextEOClip(this.cgContextRef);
            }
            else
            {
                CGContextRef.CGContextClip(this.cgContextRef);
            }

            CGContextRef.CGContextDrawRadialGradient(
                this.cgContextRef,
                this.fill.cgGradientRef,
                this.fill.startPoint,
                this.fill.startRadius,
                this.fill.endPoint,
                this.fill.endRadius);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
    }

    void IRectDrawingContext.FillRect(Rect rect)
    {
        if (this.fill.style == PaintStyle.SolidColor)
        {
            CGContextRef.CGContextFillRect(this.cgContextRef, rect);
        }
        else if (this.fill.style == PaintStyle.LinearGradient)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextClipToRect(this.cgContextRef, rect);
            CGContextRef.CGContextDrawLinearGradient(
                this.cgContextRef,
                this.stroke.cgGradientRef,
                this.stroke.startPoint,
                this.stroke.endPoint);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else if (this.fill.style == PaintStyle.RadialGradient)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextClipToRect(this.cgContextRef, rect);
            CGContextRef.CGContextDrawRadialGradient(
                this.cgContextRef,
                this.stroke.cgGradientRef,
                this.stroke.startPoint,
                this.stroke.startRadius,
                this.stroke.endPoint,
                this.stroke.endRadius);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else if (this.fill.style == PaintStyle.BitmapBrush)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextClipToRect(this.cgContextRef, rect);
            TileImagePattern(this.fill);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
    }

    void IGlyphPathBuilder.LineTo(Point to) =>
        path2d.LineTo(to);

    void IGlyphPathBuilder.MoveTo(Point to) =>
        path2d.MoveTo(to);

    void IPathBuilder.Rect(Rect rect) =>
        path2d.Rect(rect);

    void ITransformContext.Rotate(NFloat angle)
    {
        this.baseTransform ??= CGContextRef.CGContextGetCTM(this.cgContextRef);
        CGContextRef.CGContextRotateCTM(this.cgContextRef, angle);
    }

    void IPathBuilder.RoundRect(Rect rect, NFloat radius) =>
        path2d.RoundRect(rect, radius);

    void IPathBuilder.RoundRect(Rect rect, CornerRadius radius) =>
        path2d.RoundRect(rect, radius);

    void ITransformContext.Scale(Vector vector)
    {
        this.baseTransform ??= CGContextRef.CGContextGetCTM(this.cgContextRef);
        CGContextRef.CGContextScaleCTM(this.cgContextRef, vector.X, vector.Y);
    }

    void IPenContext.SetFill(Color color)
    {
        this.fill.Set(color);
        CGContextRef.CGContextSetRGBFillColor(this.cgContextRef, color.Red, color.Green, color.Blue, color.Alpha);
    }

    void IPenContext.SetFill(LinearGradient linearGradient) =>
        this.fill.Set(linearGradient);

    void IPenContext.SetFill(RadialGradient radialGradient) =>
        this.fill.Set(radialGradient);

    void IPenContext.SetStroke(Color color)
    {
        this.stroke.Set(color);
        CGContextRef.CGContextSetRGBStrokeColor(this.cgContextRef, color.Red, color.Green, color.Blue, color.Alpha);
    }

    void IPenContext.SetStroke(LinearGradient linearGradient) =>
        this.stroke.Set(linearGradient);

    void IPenContext.SetStroke(RadialGradient radialGradient) =>
        this.stroke.Set(radialGradient);

    void ITransformContext.SetTransform(AffineTransform transform)
    {
        this.baseTransform ??= CGContextRef.CGContextGetCTM(this.cgContextRef);
        var invert = CGContextRef.CGContextGetCTM(this.cgContextRef).Invert;
        CGContextRef.CGContextConcatCTM(this.cgContextRef, invert);
        CGContextRef.CGContextConcatCTM(this.cgContextRef, this.baseTransform.Value);
        CGContextRef.CGContextConcatCTM(this.cgContextRef, transform);
    }

    void IPathDrawing.Stroke()
    {
        ReplayPath();

        if (this.stroke.style == PaintStyle.SolidColor)
        {
            CGContextRef.CGContextStrokePath(this.cgContextRef);
        }
        else if (this.stroke.style == PaintStyle.LinearGradient)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextReplacePathWithStrokedPath(this.cgContextRef);
            CGContextRef.CGContextClip(this.cgContextRef);
            CGContextRef.CGContextDrawLinearGradient(
                this.cgContextRef,
                this.stroke.cgGradientRef,
                this.stroke.startPoint,
                this.stroke.endPoint);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else if (this.stroke.style == PaintStyle.RadialGradient)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextReplacePathWithStrokedPath(this.cgContextRef);
            CGContextRef.CGContextClip(this.cgContextRef);
            CGContextRef.CGContextDrawRadialGradient(
                this.cgContextRef,
                this.stroke.cgGradientRef,
                this.stroke.startPoint,
                this.stroke.startRadius,
                this.stroke.endPoint,
                this.stroke.endRadius);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else if (this.stroke.style == PaintStyle.BitmapBrush)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextReplacePathWithStrokedPath(this.cgContextRef);
            CGContextRef.CGContextClip(this.cgContextRef);
            TileImagePattern(this.stroke);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
    }

    void IRectDrawingContext.StrokeRect(Rect rect)
    {
        if (this.stroke.style == PaintStyle.SolidColor)
        {
            CGContextRef.CGContextStrokeRect(this.cgContextRef, rect);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    void ITransformContext.Transform(AffineTransform matrix)
    {
        this.baseTransform ??= CGContextRef.CGContextGetCTM(this.cgContextRef);
        CGContextRef.CGContextConcatCTM(this.cgContextRef, matrix);
    }

    void ITransformContext.Translate(Vector vector)
    {
        this.baseTransform ??= CGContextRef.CGContextGetCTM(this.cgContextRef);
        CGContextRef.CGContextTranslateCTM(this.cgContextRef, vector.X, vector.Y);
    }

    void ITextDrawingContext.FillText(string text, Point pos)
    {
        using var nsStringRef = new CFStringRef(text);
        using var attributes = new CFMutableDictionaryRef();

        using var foreground = new NSColorRef(this.fill.color);
        attributes.SetValue(NSAttributedString.Key.ForegroundColor, foreground);

        if (textMeasure.nsFont != 0)
        {
            attributes.SetValue(NSAttributedString.Key.Font, textMeasure.nsFont);
        }

        // === Horizontal alignment ===
        Vector offset = (0, 0);
        if (this.TextAlign != TextAlign.Left && this.TextAlign != TextAlign.Start)
        {
            var size = ObjC.objc_msgSend_retCGSize(nsStringRef, NSString.SizeWithAttributesSel, attributes);

            switch (this.TextAlign)
            {
                case TextAlign.Center: offset.X -= size.Width * 0.5f; break;

                case TextAlign.End: offset.X -= size.Width; break;
                case TextAlign.Right: offset.X -= size.Width; break;

                case TextAlign.Left: break;
                case TextAlign.Start: break;
            }
        }

        // === Vertical alignment ===
        if (this.TextBaseline != TextBaseline.Top)
        {
            if (textMeasure.nsFont != 0)
            {
                var ctFont = new CTFontRef(textMeasure.nsFont);

                var ascent = ctFont.Ascent;
                var descent = ctFont.Descent;
                var leading = ctFont.Leading;
                var lineHeight = ascent + descent + leading;

                offset.Y -= this.TextBaseline switch
                {
                    TextBaseline.Top         => 0f, // Top already matches
                    TextBaseline.Middle      => ascent * 0.5f + descent * 0.5f,
                    TextBaseline.Alphabetic  => ascent,
                    TextBaseline.Hanging     => ascent - ctFont.CapHeight, // Cap height below top
                    TextBaseline.Ideographic => ascent - ctFont.XHeight * 1.25f,
                    TextBaseline.Bottom      => ascent + descent,
                    _                        => ascent
                };
            }
        }

        // === Final draw ===
        NSString.objc_msgSend(nsStringRef, NSString.DrawAtPointWithAttributesSel, pos + offset, attributes);
    }

    void ITextDrawingContext.FillText(ReadOnlySpan<char> text, Point pos)
    {
        using var nsStringRef = new CFStringRef(text);
        using var attributes = new CFMutableDictionaryRef();

        using var foreground = new NSColorRef(this.fill.color);
        attributes.SetValue(NSAttributedString.Key.ForegroundColor, foreground);

        if (textMeasure.nsFont != 0)
        {
            attributes.SetValue(NSAttributedString.Key.Font, textMeasure.nsFont);
        }

        // === Horizontal alignment ===
        Vector offset = (0, 0);
        if (this.TextAlign != TextAlign.Left && this.TextAlign != TextAlign.Start)
        {
            var size = ObjC.objc_msgSend_retCGSize(nsStringRef, NSString.SizeWithAttributesSel, attributes);

            switch (this.TextAlign)
            {
                case TextAlign.Center: offset.X -= size.Width * 0.5f; break;

                case TextAlign.End: offset.X -= size.Width; break;
                case TextAlign.Right: offset.X -= size.Width; break;

                case TextAlign.Left: break;
                case TextAlign.Start: break;
            }
        }

        // === Vertical alignment ===
        if (this.TextBaseline != TextBaseline.Top)
        {
            if (textMeasure.nsFont != 0)
            {
                var ctFont = new CTFontRef(textMeasure.nsFont);

                var ascent = ctFont.Ascent;
                var descent = ctFont.Descent;
                var leading = ctFont.Leading;
                var lineHeight = ascent + descent + leading;

                offset.Y -= this.TextBaseline switch
                {
                    TextBaseline.Top         => 0f, // Top already matches
                    TextBaseline.Middle      => ascent * 0.5f + descent * 0.5f,
                    TextBaseline.Alphabetic  => ascent,
                    TextBaseline.Hanging     => ascent - ctFont.CapHeight, // Cap height below top
                    TextBaseline.Ideographic => ascent - ctFont.XHeight * 1.25f,
                    TextBaseline.Bottom      => ascent + descent,
                    _                        => ascent
                };
            }
        }

        // === Final draw ===
        NSString.objc_msgSend(nsStringRef, NSString.DrawAtPointWithAttributesSel, pos + offset, attributes);
    }

    TextMetrics ITextMeasureContext.MeasureText(string text) => textMeasure.MeasureText(text);

    TextMetrics ITextMeasureContext.MeasureText(ReadOnlySpan<char> text) => textMeasure.MeasureText(text);

    void ITextMeasureContext.SetFont(Font font) => textMeasure.SetFont(font);

    private struct Paint
    {
        public PaintStyle style;
        public Color color;
        public nint cgGradientRef;
        public Point startPoint;
        public Point endPoint;
        public NFloat startRadius;
        public NFloat endRadius;
        // BitmapBrush — borrowed reference, NOT owned (MacOSImageResource owns the CGImageRef)
        public nint cgImage;
        public uint imgWidth;
        public uint imgHeight;
        public PatternRepeat patternRepeat;

        public void Set(Color color)
        {
            this.Reset(PaintStyle.SolidColor);
            this.color = color;
        }

        public void Set(LinearGradient linearGradient)
        {
            this.Reset(PaintStyle.LinearGradient);
            this.cgGradientRef = new CGGradientRef(linearGradient.GradientStops);
            this.startPoint = linearGradient.StartPoint;
            this.endPoint = linearGradient.EndPoint;
        }

        public void Set(RadialGradient radialGradient)
        {
            this.Reset(PaintStyle.RadialGradient);
            this.cgGradientRef = new CGGradientRef(radialGradient.GradientStops);
            this.startPoint = radialGradient.StartCenter;
            this.startRadius = radialGradient.StartRadius;
            this.endPoint = radialGradient.EndCenter;
            this.endRadius = radialGradient.EndRadius;
        }

        public void Set(ImagePattern pattern)
        {
            if (pattern.Image is not MacOSImage img || img.Resource == null || img.Resource.CGImage == 0)
                return;
            this.Reset(PaintStyle.BitmapBrush);
            this.cgImage      = img.Resource.CGImage;
            this.imgWidth     = img.Resource.Width;
            this.imgHeight    = img.Resource.Height;
            this.patternRepeat = pattern.Repetition;
        }

        public void Reset(PaintStyle style = PaintStyle.SolidColor)
        {
            if (this.cgGradientRef != 0)
            {
                CGGradientRef.CGGradientRelease(cgGradientRef);
                this.cgGradientRef = 0;
            }
            this.cgImage = 0; // borrowed — do not release
            this.style = style;
        }
    }

    public void SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    void IPenContext.SetFill(ImagePattern pattern) => this.fill.Set(pattern);
    void IPenContext.SetStroke(ImagePattern pattern) => this.stroke.Set(pattern);

    void IImageDrawingContext.DrawImage(IImage image, Rect dest)
    {
        if (image is not MacOSImage img || img.Resource == null || img.Resource.CGImage == 0)
            return;
        DrawCGImageFlipped(img.Resource.CGImage, dest, 1f);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect dest, NFloat opacity)
    {
        if (image is not MacOSImage img || img.Resource == null || img.Resource.CGImage == 0)
            return;
        DrawCGImageFlipped(img.Resource.CGImage, dest, opacity);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect source, Rect dest, NFloat opacity)
    {
        if (image is not MacOSImage img || img.Resource == null || img.Resource.CGImage == 0)
            return;
        // Crop to source rect (in image pixel coordinates) then draw cropped image.
        using var cropped = CGImageRef.CreateWithImageInRect(img.Resource.CGImage, source);
        DrawCGImageFlipped(cropped, dest, opacity);
    }

    /// <summary>
    /// Tiles <paramref name="paint"/>'s CGImage across the current clip bounding box.
    /// Must be called inside a Save/clip/Restore block so the clip is already applied.
    /// </summary>
    private void TileImagePattern(in Paint paint)
    {
        if (paint.cgImage == 0 || paint.imgWidth == 0 || paint.imgHeight == 0)
            return;

        CGRect clip = CGContextRef.CGContextGetClipBoundingBox(this.cgContextRef);
        NFloat imgW = (NFloat)paint.imgWidth;
        NFloat imgH = (NFloat)paint.imgHeight;
        bool tileX = paint.patternRepeat is PatternRepeat.Repeat or PatternRepeat.RepeatX;
        bool tileY = paint.patternRepeat is PatternRepeat.Repeat or PatternRepeat.RepeatY;

        // Anchor tiles to (0,0) in the current user-space — mirrors browser createPattern behaviour.
        // For each axis: snap the loop start back to the first tile-grid multiple that still covers
        // the clip edge, then run forward until the clip is fully covered (or just one tile if not tiling).
        NFloat startX = tileX ? NFloat.Floor(clip.Origin.X / imgW) * imgW : 0f;
        NFloat startY = tileY ? NFloat.Floor(clip.Origin.Y / imgH) * imgH : 0f;
        NFloat xEnd   = tileX ? clip.Origin.X + clip.Size.Width  : imgW;
        NFloat yEnd   = tileY ? clip.Origin.Y + clip.Size.Height : imgH;

        for (NFloat ty = startY; ty < yEnd; ty += imgH)
            for (NFloat tx = startX; tx < xEnd; tx += imgW)
                DrawCGImageFlipped(paint.cgImage, new Rect(tx, ty, imgW, imgH), 1f);
    }

    private void DrawCGImageFlipped(nint cgImage, Rect dest, NFloat opacity)
    {
        CGContextRef.CGContextSaveGState(this.cgContextRef);
        if (opacity != 1f)
            CGContextRef.CGContextSetAlpha(this.cgContextRef, opacity);
        // CGContextDrawImage places the image bottom-up in a Y-down flipped view.
        // Correct by translating to the bottom edge of dest and scaling Y by -1.
        CGContextRef.CGContextTranslateCTM(this.cgContextRef, dest.X, dest.Y + dest.Height);
        CGContextRef.CGContextScaleCTM(this.cgContextRef, 1, -1);
        CGContextRef.CGContextDrawImage(this.cgContextRef, new CGRect(0, 0, dest.Width, dest.Height), cgImage);
        CGContextRef.CGContextRestoreGState(this.cgContextRef);
    }
}
