using System.Runtime.InteropServices;
using System;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Filter;
using System.Collections.Generic;
using Xui.Runtime.IOS;
using static Xui.Runtime.IOS.UIKit;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.Foundation;
using static Xui.Runtime.IOS.CoreText;
using Xui.Core.Math2D;

namespace Xui.Runtime.IOS.Actual;

public partial class IOSDrawingContext : IContext
{
    private nint cgContextRef;
    private FilterState filter;
    private readonly Stack<(Paint Fill, Paint Stroke, FilterState Filter)> paintStates = new();

    public void SetFilter() => filter = default;
    public void SetFilter(Brightness brightness) => filter = new(brightness);

    private void ResetPaintStates()
    {
        while (paintStates.TryPop(out var state))
        {
            state.Fill.Reset();
            state.Stroke.Reset();
        }
        filter = default;
    }

    private Paint fill;

    private Paint stroke;

    internal readonly IOSTextMeasureContext textMeasure;

    private AffineTransform? baseTransform;

    public IOSDrawingContext(IFontSource? fontSource = null) => textMeasure = new IOSTextMeasureContext(fontSource);

    NFloat IPenContext.GlobalAlpha { set => CGContextRef.CGContextSetAlpha(this.cgContextRef, value); }

    LineCap IPenContext.LineCap { set => CGContextRef.CGContextSetLineCap(this.cgContextRef, (CGLineCap)value); }
    LineJoin IPenContext.LineJoin { set => CGContextRef.CGContextSetLineJoin(this.cgContextRef, (CGLineJoin)value); }
    NFloat IPenContext.LineWidth { set => CGContextRef.CGContextSetLineWidth(this.cgContextRef, value); }
    NFloat IPenContext.MiterLimit { set => CGContextRef.CGContextSetMiterLimit(this.cgContextRef, value); }

    private NFloat lineDashOffset;
    private NFloat[] lineDashSegments = [];

    NFloat IPenContext.LineDashOffset
    {
        set
        {
            if (lineDashOffset == value)
                return;
            lineDashOffset = value;
            ApplyLineDash();
        }
    }

    public TextAlign TextAlign { get => textMeasure.TextAlign; set => textMeasure.TextAlign = value; }

    public TextBaseline TextBaseline { get; set; }

    public IOSDrawingContext Bind()
    {
        // this.cgContextRef = NSGraphicsContext.CurrentCGContext.Self;
        this.cgContextRef = Xui.Runtime.IOS.UIKit.UIGraphicsGetCurrentContext();
        this.fill.Reset();
        this.stroke.Reset();
        this.baseTransform = null;
        ResetPaintStates();
        this.path2d.BeginPath();

        return this;
    }

    void IStateContext.Restore()
    {
        if (!paintStates.TryPop(out var state))
            return;
        fill.Reset();
        stroke.Reset();
        (fill, stroke, filter) = state;
        CGContextRef.CGContextRestoreGState(this.cgContextRef);
    }

    void IStateContext.Save()
    {
        // The native state saves solid paints; retain our gradient handles and text paint too.
        paintStates.Push((fill, stroke, filter));
        if (fill.cgGradientRef != 0) CFRetain(fill.cgGradientRef);
        if (stroke.cgGradientRef != 0) CFRetain(stroke.cgGradientRef);
        CGContextRef.CGContextSaveGState(this.cgContextRef);
    }

    void IPenContext.SetLineDash(ReadOnlySpan<NFloat> segments)
    {
        lineDashSegments = segments.ToArray();
        ApplyLineDash();
    }

    private void ApplyLineDash()
    {
        var segments = lineDashSegments.AsSpan();
        if (segments.IsEmpty)
        {
            CGContextRef.CGContextSetLineDash(this.cgContextRef, lineDashOffset, nint.Zero, nint.Zero);
        }
        else if (segments.Length % 2 == 0)
        {
            CGContextRef.CGContextSetLineDash(this.cgContextRef, lineDashOffset, ref MemoryMarshal.GetReference(segments), segments.Length);
        }
        else
        {
            Span<NFloat> mirrored = stackalloc NFloat[segments.Length * 2];
            for(var i = 0; i < segments.Length; i++)
            {
                mirrored[i] = mirrored[i + segments.Length] = segments[i];
            }
            CGContextRef.CGContextSetLineDash(this.cgContextRef, lineDashOffset, ref MemoryMarshal.GetReference(mirrored), mirrored.Length);
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
        ResetPaintStates();

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
        else if (this.fill.style == PaintStyle.RadialGradient)
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
                this.fill.cgGradientRef,
                this.fill.startPoint,
                this.fill.endPoint);
            CGContextRef.CGContextRestoreGState(this.cgContextRef);
        }
        else if (this.fill.style == PaintStyle.RadialGradient)
        {
            CGContextRef.CGContextSaveGState(this.cgContextRef);
            CGContextRef.CGContextClipToRect(this.cgContextRef, rect);
            CGContextRef.CGContextDrawRadialGradient(
                this.cgContextRef,
                this.fill.cgGradientRef,
                this.fill.startPoint,
                this.fill.startRadius,
                this.fill.endPoint,
                this.fill.endRadius);
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
        color = filter.Apply(color);
        this.fill.Set(color);
        CGContextRef.CGContextSetRGBFillColor(this.cgContextRef, color.Red, color.Green, color.Blue, color.Alpha);
    }

    void IPenContext.SetFill(LinearGradient linearGradient) =>
        this.fill.Set(linearGradient, filter);

    void IPenContext.SetFill(RadialGradient radialGradient) =>
        this.fill.Set(radialGradient, filter);

    void IPenContext.SetStroke(Color color)
    {
        color = filter.Apply(color);
        this.stroke.Set(color);
        CGContextRef.CGContextSetRGBStrokeColor(this.cgContextRef, color.Red, color.Green, color.Blue, color.Alpha);
    }

    void IPenContext.SetStroke(LinearGradient linearGradient) =>
        this.stroke.Set(linearGradient, filter);

    void IPenContext.SetStroke(RadialGradient radialGradient) =>
        this.stroke.Set(radialGradient, filter);

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
        FillText(nsStringRef, pos);
    }

    void ITextDrawingContext.FillText(ReadOnlySpan<char> text, Point pos)
    {
        using var nsStringRef = new CFStringRef(text);
        FillText(nsStringRef, pos);
    }

    private void FillText(CFStringRef nsStringRef, Point pos)
    {
        if (this.fill.style != PaintStyle.SolidColor)
        {
            FillTextWithPaint(nsStringRef, pos);
            return;
        }

        using var attributes = new CFMutableDictionaryRef();

        using var foreground = new UIColorRef(this.fill.color);
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

    private void FillTextWithPaint(CFStringRef text, Point pos)
    {
        using var attributes = new CFMutableDictionaryRef();
        using var foreground = new UIColorRef(new Color(0, 0, 0, 255));
        attributes.SetValue(NSAttributedString.Key.ForegroundColor, foreground);
        if (textMeasure.nsFont != 0)
            attributes.SetValue(NSAttributedString.Key.Font, textMeasure.nsFont);

        var offset = Vector.Zero;
        if (TextAlign != TextAlign.Left && TextAlign != TextAlign.Start)
        {
            var size = ObjC.objc_msgSend_retCGSize(text, NSString.SizeWithAttributesSel, attributes);
            offset.X -= TextAlign switch
            {
                TextAlign.Center => size.Width * 0.5f,
                TextAlign.End or TextAlign.Right => size.Width,
                _ => 0f,
            };
        }

        if (TextBaseline != TextBaseline.Top && textMeasure.nsFont != 0)
        {
            var ctFont = new CTFontRef(textMeasure.nsFont);
            var ascent = ctFont.Ascent;
            var descent = ctFont.Descent;
            offset.Y -= TextBaseline switch
            {
                TextBaseline.Middle => (ascent + descent) * 0.5f,
                TextBaseline.Alphabetic => ascent,
                TextBaseline.Hanging => ascent - ctFont.CapHeight,
                TextBaseline.Ideographic => ascent - ctFont.XHeight * 1.25f,
                TextBaseline.Bottom => ascent + descent,
                _ => ascent,
            };
        }

        CGContextRef.CGContextSaveGState(cgContextRef);
        CGContextRef.CGContextBeginTransparencyLayer(cgContextRef, nint.Zero);
        NSString.objc_msgSend(text, NSString.DrawAtPointWithAttributesSel, pos + offset, attributes);
        CGContextRef.CGContextSetBlendMode(cgContextRef, CGBlendMode.SourceIn);
        if (fill.style == PaintStyle.LinearGradient)
            CGContextRef.CGContextDrawLinearGradient(cgContextRef, fill.cgGradientRef, fill.startPoint, fill.endPoint);
        else if (fill.style == PaintStyle.RadialGradient)
            CGContextRef.CGContextDrawRadialGradient(cgContextRef, fill.cgGradientRef, fill.startPoint, fill.startRadius, fill.endPoint, fill.endRadius);
        CGContextRef.CGContextEndTransparencyLayer(cgContextRef);
        CGContextRef.CGContextRestoreGState(cgContextRef);
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
        // Borrowed CGImage reference; IOSImageResource owns its lifetime.
        public nint cgImage;
        public uint imageWidth;
        public uint imageHeight;
        public PatternRepeat patternRepeat;

        public void Set(Color color)
        {
            this.Reset(PaintStyle.SolidColor);
            this.color = color;
        }

        public void Set(LinearGradient linearGradient, FilterState filter)
        {
            this.Reset(PaintStyle.LinearGradient);
            this.cgGradientRef = new CGGradientRef(linearGradient.GradientStops, filter);
            this.startPoint = linearGradient.StartPoint;
            this.endPoint = linearGradient.EndPoint;
        }

        public void Set(RadialGradient radialGradient, FilterState filter)
        {
            this.Reset(PaintStyle.RadialGradient);
            this.cgGradientRef = new CGGradientRef(radialGradient.GradientStops, filter);
            this.startPoint = radialGradient.StartCenter;
            this.startRadius = radialGradient.StartRadius;
            this.endPoint = radialGradient.EndCenter;
            this.endRadius = radialGradient.EndRadius;
        }

        public void Set(ImagePattern pattern)
        {
            if (pattern.Image is not IOSImage image || image.Resource?.CGImage is null)
                return;

            Reset(PaintStyle.BitmapBrush);
            cgImage = image.Resource.CGImage;
            imageWidth = image.Resource.Width;
            imageHeight = image.Resource.Height;
            patternRepeat = pattern.Repetition;
        }

        public void Reset(PaintStyle style = PaintStyle.SolidColor)
        {
            if (this.cgGradientRef != 0)
            {
                CGGradientRef.CGGradientRelease(cgGradientRef);
                this.cgGradientRef = 0;
            }
            cgImage = 0;
            imageWidth = 0;
            imageHeight = 0;
            this.style = style;
        }
    }

    /// <summary>
    /// <code>
    /// NSFontWeightUltraLight: -0.800000 - 100 Thin (Hairline)
    /// NSFontWeightThin: -0.600000       - 200 Extra Light (Ultra Light)
    /// NSFontWeightLight: -0.400000      - 300 Light
    /// NSFontWeightRegular: 0.000000     - 400 Normal (Regular)
    /// NSFontWeightMedium: 0.230000      - 500 Medium
    /// NSFontWeightSemibold: 0.300000    - 600 Semi Bold (Demi Bold)
    /// NSFontWeightBold: 0.400000        - 700 Bold
    ///                                   - 800 Extra Bold (Ultra Bold)
    /// NSFontWeightHeavy: 0.560000       - 900 Black (Heavy)
    ///                                   - 950 Extra Black (Ultra Black)
    /// </code>
    /// </summary>
    /// <param name="webWeight"></param>
    /// <returns></returns>
    private NFloat MapWebWeightToCTWeight(NFloat webWeight)
    {
        if (webWeight == 400) return 0;
        webWeight = NFloat.Clamp(webWeight, 1, 1000);
        if (webWeight < 100f) return NFloat.Lerp(-1, -0.8f, (webWeight - 1f) / 100f);
        if (webWeight <= 300f) return NFloat.Lerp(-0.8f, -0.4f, (webWeight - 100f) / 200f);
        if (webWeight <= 400f) return NFloat.Lerp(-0.4f, 0f, (webWeight - 300f) / 100f);
        if (webWeight <= 500f) return NFloat.Lerp(0f, 0.23f, (webWeight - 400f) / 100f);
        if (webWeight <= 600f) return NFloat.Lerp(0.23f, 0.3f, (webWeight - 500f) / 100f);
        if (webWeight <= 700f) return NFloat.Lerp(0.3f, 0.4f, (webWeight - 600f) / 100f);
        if (webWeight <= 900f) return NFloat.Lerp(0.4f, 0.56f, (webWeight - 700f) / 200f);
        return NFloat.Lerp(0.56f, 1f, (webWeight - 900f) / 100f);
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
        if (image is IOSImage iosImage && iosImage.Resource?.CGImage is { } cgImage)
            DrawCGImageFlipped(cgImage, dest, 1);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect dest, NFloat opacity)
    {
        if (image is IOSImage iosImage && iosImage.Resource?.CGImage is { } cgImage)
            DrawCGImageFlipped(cgImage, dest, opacity);
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect source, Rect dest, NFloat opacity)
    {
        if (image is not IOSImage iosImage || iosImage.Resource?.CGImage is not { } cgImage)
            return;

        using var cropped = CGImageRef.CreateWithImageInRect(cgImage, source);
        DrawCGImageFlipped(cropped, dest, opacity);
    }

    private void TileImagePattern(in Paint paint)
    {
        if (paint.cgImage == 0 || paint.imageWidth == 0 || paint.imageHeight == 0)
            return;

        var clip = CGContextRef.CGContextGetClipBoundingBox(cgContextRef);
        var imageWidth = (NFloat)paint.imageWidth;
        var imageHeight = (NFloat)paint.imageHeight;
        var tileX = paint.patternRepeat is PatternRepeat.Repeat or PatternRepeat.RepeatX;
        var tileY = paint.patternRepeat is PatternRepeat.Repeat or PatternRepeat.RepeatY;
        var startX = tileX ? NFloat.Floor(clip.Origin.X / imageWidth) * imageWidth : 0;
        var startY = tileY ? NFloat.Floor(clip.Origin.Y / imageHeight) * imageHeight : 0;
        var endX = tileX ? clip.Origin.X + clip.Size.Width : imageWidth;
        var endY = tileY ? clip.Origin.Y + clip.Size.Height : imageHeight;

        for (var y = startY; y < endY; y += imageHeight)
            for (var x = startX; x < endX; x += imageWidth)
                DrawCGImageFlipped(paint.cgImage, new Rect(x, y, imageWidth, imageHeight), 1);
    }

    private void DrawCGImageFlipped(nint image, Rect destination, NFloat opacity)
    {
        CGContextRef.CGContextSaveGState(cgContextRef);
        if (opacity != 1)
            CGContextRef.CGContextSetAlpha(cgContextRef, opacity);
        CGContextRef.CGContextTranslateCTM(cgContextRef, destination.X, destination.Y + destination.Height);
        CGContextRef.CGContextScaleCTM(cgContextRef, 1, -1);
        CGContextRef.CGContextDrawImage(cgContextRef, new CGRect(0, 0, destination.Width, destination.Height), image);
        CGContextRef.CGContextRestoreGState(cgContextRef);
    }
}
