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

public class MacOSDrawingContext : IContext
{
    private nint cgContextRef;

    private Paint fill;

    private Paint stroke;

    private nint nsFont;

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

    public TextAlign TextAlign { get; set; }

    public TextBaseline TextBaseline { get; set; }

    public MacOSDrawingContext Bind()
    {
        this.cgContextRef = NSGraphicsContext.CurrentCGContext.Self;
        this.fill.Reset();
        this.stroke.Reset();
        this.baseTransform = null;

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
        CGContextRef.CGContextAddArc(this.cgContextRef, center.X, center.Y, radius, startAngle, endAngle, (int)winding);

    void IPathBuilder.ArcTo(Point cp1, Point cp2, NFloat radius) =>
        CGContextRef.CGContextAddArcToPoint(this.cgContextRef, cp1.X, cp1.Y, cp2.X, cp2.Y, radius);

    void IPathBuilder.BeginPath() =>
        CGContextRef.CGContextBeginPath(this.cgContextRef);

    void IPathClipping.Clip() =>
        CGContextRef.CGContextClip(this.cgContextRef);

    void IGlyphPathBuilder.ClosePath() =>
        CGContextRef.CGContextClosePath(this.cgContextRef);

    void IGlyphPathBuilder.CurveTo(Point cp1, Point to) =>
        CGContextRef.CGContextAddQuadCurveToPoint(this.cgContextRef, cp1.X, cp1.Y, to.X, to.Y);

    void IPathBuilder.CurveTo(Point cp1, Point cp2, Point to) =>
        CGContextRef.CGContextAddCurveToPoint(this.cgContextRef, cp1.X, cp1.Y, cp2.X, cp2.Y, to.X, to.Y);

    void IDisposable.Dispose()
    {
        if (this.cgContextRef != nint.Zero)
        {
            this.cgContextRef = nint.Zero;
        }

        this.fill.Reset();
        this.stroke.Reset();

        this.baseTransform = null;

        if (this.nsFont != 0)
        {
            CoreFoundation.CFRelease(this.nsFont);
            this.nsFont = 0;
        }
    }

    void IPathBuilder.Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding)
    {
        CGContextRef.CGContextSaveGState(this.cgContextRef);
        CGContextRef.CGContextTranslateCTM(this.cgContextRef, center.X, center.Y);
        CGContextRef.CGContextRotateCTM(this.cgContextRef, rotation);
        CGContextRef.CGContextScaleCTM(this.cgContextRef, radiusX, radiusY);
        CGContextRef.CGContextAddArc(this.cgContextRef, 0, 0, 1, endAngle, startAngle, (int)winding);
        CGContextRef.CGContextRestoreGState(this.cgContextRef);
    }

    void IPathDrawing.Fill(FillRule fillRule)
    {
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
    }

    void IGlyphPathBuilder.LineTo(Point to) =>
        CGContextRef.CGContextAddLineToPoint(this.cgContextRef, to.X, to.Y);

    void IGlyphPathBuilder.MoveTo(Point to) =>
        CGContextRef.CGContextMoveToPoint(this.cgContextRef, to.X, to.Y);

    void IPathBuilder.Rect(Rect rect) =>
        CGContextRef.CGContextAddRect(this.cgContextRef, rect);

    void ITransformContext.Rotate(NFloat angle)
    {
        this.baseTransform ??= CGContextRef.CGContextGetCTM(this.cgContextRef);
        CGContextRef.CGContextRotateCTM(this.cgContextRef, angle);
    }

    void IPathBuilder.RoundRect(Rect rect, NFloat radius)
    {
        using var cgPathRef = CGPathRef.CreateWithRoundedRect(rect, radius);
        CGContextRef.CGContextAddPath(this.cgContextRef, cgPathRef);
    }

    void IPathBuilder.RoundRect(Rect rect, CornerRadius radius)
    {
        var context = (IContext)this;

        // TODO: Clap radius to available space in rect

        if (radius.TopLeft == 0)
        {
            context.MoveTo(rect.TopLeft);
        }
        else
        {
            context.MoveTo(new (rect.X, rect.Y + radius.TopLeft));
            context.ArcTo(rect.TopLeft, rect.TopRight, radius.TopLeft);
        }

        if (radius.TopRight == 0)
        {
            context.LineTo(rect.TopRight);
        }
        else
        {
            context.ArcTo(rect.TopRight, rect.BottomRight, radius.TopRight);
        }

        if (radius.BottomRight == 0)
        {
            context.LineTo(rect.BottomRight);
        }
        else
        {
            context.ArcTo(rect.BottomRight, rect.BottomLeft, radius.BottomRight);
        }

        if (radius.BottomLeft == 0)
        {
            context.LineTo(rect.BottomLeft);
        }
        else
        {
            context.ArcTo(rect.BottomLeft, rect.TopLeft, radius.BottomLeft);
        }

        context.ClosePath();
    }

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

        if (this.nsFont != 0)
        {
            attributes.SetValue(NSAttributedString.Key.Font, this.nsFont);
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
            if (this.nsFont != 0)
            {
                var ctFont = new CTFontRef(this.nsFont);

                var ascent = ctFont.Ascent;
                var descent = ctFont.Descent;
                var leading = ctFont.Leading;
                var lineHeight = ascent + descent + leading;

                offset.Y -= this.TextBaseline switch
                {
                    TextBaseline.Top         => 0f, // Top already matches
                    TextBaseline.Middle      => -ascent * 0.5f + descent * 0.5f,
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

    TextMetrics ITextMeasureContext.MeasureText(string text)
    {
        using var nsStringRef = new CFStringRef(text);
        using var attributes = new CFMutableDictionaryRef();
        using var foreground = new NSColorRef(this.fill.color);

        attributes.SetValue(NSAttributedString.Key.ForegroundColor, foreground);
        if (this.nsFont != 0)
            attributes.SetValue(NSAttributedString.Key.Font, this.nsFont);

        using var attrString = new NSAttributedStringRef(nsStringRef, attributes);
        using var line = CTLineRef.Create(attrString);

        // === Extract Line Metrics (accurate width, left/right bounds, ascent/descent) ===
        Rect glyphBounds = line.GetBounds(CoreText.CTLineRef.BoundsOptions.UseGlyphPathBounds);
        Rect opticalBounds = line.GetBounds(CoreText.CTLineRef.BoundsOptions.UseOpticalBounds);

        NFloat alignOffset = this.TextAlign switch
        {
            TextAlign.Center => opticalBounds.Width * 0.5f,
            TextAlign.Right  => opticalBounds.Width,
            _ => 0f
        };

        var lineMetrics = new LineMetrics(
            width: opticalBounds.Width,
            left: alignOffset,
            right: opticalBounds.Width - alignOffset,
            ascent: glyphBounds.Height + glyphBounds.Y,
            descent: -glyphBounds.Y
        );

        // === Extract Font Metrics ===
        FontMetrics font;
        if (this.nsFont != 0)
        {
            var ct = new CTFontRef(this.nsFont);

            var ascent = ct.Ascent;
            var descent = ct.Descent;

            // Too complicated, reads font tables, accurate but...
            // font = ct.FontMetrics;

            font = new FontMetrics(
                fontAscent: ascent,
                fontDescent: descent,
                emAscent: ascent,
                emDescent: descent,
                alphabeticBaseline: 0,
                hangingBaseline: -ascent,
                ideographicBaseline: descent
            );
        }
        else
        {
            font = default;
        }

        return new TextMetrics(lineMetrics, font);
    }

    void ITextMeasureContext.SetFont(Font font)
    {
        if (this.nsFont != 0)
        {
            CoreFoundation.CFRelease(this.nsFont);
            this.nsFont = 0;
        }

        try
        {
            using var attributes = new CFMutableDictionaryRef();

            if (font.FontFamily.Length >= 1)
            {
                using var fontFamilyNameRef = new CFStringRef(font.FontFamily[0]);
                attributes.SetValue(CTFontDescriptor.FontAttributes.FamilyName, fontFamilyNameRef);

                if (font.FontFamily.Length > 1)
                {
                    using var nsCascadingFontArray = new CFMutableArrayRef();
                    foreach(var f in font.FontFamily)
                    {
                        using var desc = new CTFontDescriptorRef(f);
                        nsCascadingFontArray.Add(desc);
                    }
                    // TODO: Here the dictionary does not retain the array and the array is disposed...
                    attributes.SetValue(CTFontDescriptor.FontAttributes.CascadeList, nsCascadingFontArray);
                }
            }

            using var fontSizeRef = new CFNumberRef(font.FontSize);
            attributes.SetValue(CTFontDescriptor.FontAttributes.Size, fontSizeRef);

            using var fontTraits = new CFMutableDictionaryRef();

            // 400 is regular, 700 is bold set through symbolic traits
            if (font.FontWeight != 400 && font.FontWeight != 700)
            {
                NFloat webWeightToCTWeight = MapWebWeightToCTWeight(font.FontWeight);
                using var fontWeight = new CFNumberRef(webWeightToCTWeight);
                fontTraits.SetValue(CTFontDescriptor.FontTrait.Weight, fontWeight);
            }

            if (font.FontStyle.IsItalic || font.FontWeight == 700)
            {
                int symTrait = 0;
                if (font.FontStyle.IsItalic)
                {
                    symTrait |= (int)CTFontDescriptor.SymbolicTrait.Italic;
                }

                if (font.FontWeight == 700)
                {
                    symTrait |= (int)CTFontDescriptor.SymbolicTrait.Bold;
                }

                using var symTraitRef = new CFNumberRef(symTrait);
                fontTraits.SetValue(CTFontDescriptor.FontTrait.Symbolic, symTraitRef);
            }

            if (font.FontStyle.IsOblique)
            {
                var slant = new CFNumberRef(NFloat.Clamp(font.FontStyle.ObliqueAngle / 90f, -1f, 1f));
                fontTraits.SetValue(CTFontDescriptor.FontTrait.Slant, slant);
            }

            // fontTraits.SetValue(CTFontDescriptor.FontTrait.Width, /* oblique */);

            attributes.SetValue(CTFontDescriptor.FontAttributes.Traits, fontTraits);

            using var descriptor = CTFontDescriptorRef.WithAttributes(attributes);
            nint options = 0;

            // Important - do not dispose, no "using", we will keep it manually
            var ctFont = new CTFontRef(descriptor, options);
            this.nsFont = ctFont;
        }
        catch(Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private struct Paint
    {
        public PaintStyle style;
        public Color color;
        public nint cgGradientRef;
        public Point startPoint;
        public Point endPoint;
        public NFloat startRadius;
        public NFloat endRadius;

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

        public void Reset(PaintStyle style = PaintStyle.SolidColor)
        {
            if (this.cgGradientRef != 0)
            {
                CGGradientRef.CGGradientRelease(cgGradientRef);
                this.cgGradientRef = 0;
            }
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

    void IPenContext.SetFill(ImagePattern pattern) => throw new NotImplementedException();
    void IPenContext.SetStroke(ImagePattern pattern) => throw new NotImplementedException();

    void IImageDrawingContext.DrawImage(IImage image, Rect dest) => throw new NotImplementedException();
    void IImageDrawingContext.DrawImage(IImage image, Rect dest, NFloat opacity) => throw new NotImplementedException();
    void IImageDrawingContext.DrawImage(IImage image, Rect source, Rect dest, NFloat opacity) => throw new NotImplementedException();
}