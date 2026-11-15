using System;
using System.Collections.Generic;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Software.Actual;

public sealed partial class SvgDrawingContext
{
    private readonly Dictionary<SoftwareImage.Resource, string> images = new();
    private int patternCounter;
    private ImagePaint? currentFillImage, currentStrokeImage;
    private sealed record ImagePaint(SoftwareImage.Resource Image, PatternRepeat Repetition);
    private const string XLink = "xmlns:xlink=\"http://www.w3.org/1999/xlink\"";

    private static SoftwareImage.Resource ImageResource(IImage image) => image is SoftwareImage { Content: { } resource }
        ? resource : throw new NotSupportedException("SVG image drawing requires a loaded image from SoftwareImagePipeline.");

    private string EmbedImage(SoftwareImage.Resource resource)
    {
        if (images.TryGetValue(resource, out var id)) return id;
        id = "image" + (images.Count + 1);
        images.Add(resource, id);
        defsWriter.WriteLine($"    <image id=\"{id}\" width=\"{F(resource.Size.Width)}\" height=\"{F(resource.Size.Height)}\" {XLink} xlink:href=\"{resource.DataUri}\" />");
        return id;
    }

    void IPenContext.SetFill(ImagePattern pattern)
    {
        currentFillImage = new(ImageResource(pattern.Image), pattern.Repetition);
        currentFillColor = null;
        currentFillGradientId = null;
    }

    void IPenContext.SetStroke(ImagePattern pattern)
    {
        currentStrokeImage = new(ImageResource(pattern.Image), pattern.Repetition);
        currentStrokeColor = null;
        currentStrokeGradientId = null;
    }

    private string? PaintId(ImagePaint? paint, string? gradient)
    {
        if (paint is null) return gradient;
        var imageId = EmbedImage(paint.Image);
        var width = paint.Image.Size.Width;
        var height = paint.Image.Size.Height;
        nfloat x = 0, y = 0;
        if (paint.Repetition != PatternRepeat.Repeat)
        {
            // Non-repeating axes get a transparent tile covering the entire visible
            // viewport in the current user space. Compute at draw time, after transforms.
            var inverse = transform.Inverse;
            Span<Point> corners = stackalloc Point[]
            {
                inverse * new Point(0, 0), inverse * new Point(canvasSize.Width, 0),
                inverse * new Point(0, canvasSize.Height), inverse * new Point(canvasSize.Width, canvasSize.Height),
            };
            nfloat left = 0, top = 0, right = width, bottom = height;
            foreach (var point in corners)
            {
                left = nfloat.Min(left, point.X); top = nfloat.Min(top, point.Y);
                right = nfloat.Max(right, point.X); bottom = nfloat.Max(bottom, point.Y);
            }
            if (paint.Repetition is PatternRepeat.NoRepeat or PatternRepeat.RepeatY)
            { x = left - width; width = right - left + 2 * width; }
            if (paint.Repetition is PatternRepeat.NoRepeat or PatternRepeat.RepeatX)
            { y = top - height; height = bottom - top + 2 * height; }
        }
        var id = "pattern" + ++patternCounter;
        defsWriter.WriteLine($"    <pattern id=\"{id}\" patternUnits=\"userSpaceOnUse\" viewBox=\"{F(x)} {F(y)} {F(width)} {F(height)}\" preserveAspectRatio=\"none\" x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(width)}\" height=\"{F(height)}\">");
        defsWriter.WriteLine($"      <use {XLink} xlink:href=\"#{imageId}\" opacity=\"{F(nfloat.Clamp(GlobalAlpha, 0, 1))}\" />");
        defsWriter.WriteLine("    </pattern>");
        return id;
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect dest) => DrawImage(image, new Rect((0, 0), image.Size), dest, 1);
    void IImageDrawingContext.DrawImage(IImage image, Rect dest, nfloat opacity) => DrawImage(image, new Rect((0, 0), image.Size), dest, opacity);
    void IImageDrawingContext.DrawImage(IImage image, Rect source, Rect dest, nfloat opacity) => DrawImage(image, source, dest, opacity);

    private void DrawImage(IImage image, Rect source, Rect dest, nfloat opacity)
    {
        CheckCompositionDrawing();
        if (source.Width <= 0 || source.Height <= 0 || dest.Width <= 0 || dest.Height <= 0) return;
        var id = EmbedImage(ImageResource(image));
        Ident();
        bodyWriter.WriteLine($"<svg x=\"{F(dest.X)}\" y=\"{F(dest.Y)}\" width=\"{F(dest.Width)}\" height=\"{F(dest.Height)}\" viewBox=\"{F(source.X)} {F(source.Y)} {F(source.Width)} {F(source.Height)}\" preserveAspectRatio=\"none\" overflow=\"hidden\" opacity=\"{F(nfloat.Clamp(opacity, 0, 1) * nfloat.Clamp(GlobalAlpha, 0, 1))}\">");
        Ident(); bodyWriter.WriteLine($"  <use {XLink} xlink:href=\"#{id}\" />");
        Ident(); bodyWriter.WriteLine("</svg>");
    }
}
