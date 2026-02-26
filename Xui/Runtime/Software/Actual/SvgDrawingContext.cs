using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Xui.Core.Canvas;
using Xui.Core.Curves2D;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Font;

namespace Xui.Runtime.Software.Actual;

public sealed class SvgDrawingContext : IContext, IDisposable
{
    private readonly Stream stream;

    private readonly MemoryStream styleBuffer;
    private readonly MemoryStream defsBuffer;
    private readonly MemoryStream bodyBuffer;
    private readonly StreamWriter styleWriter;
    private readonly StreamWriter defsWriter;
    private readonly StreamWriter bodyWriter;

    private readonly bool keepOpen;
    private readonly Size canvasSize;
    private bool disposed = false;

    private StringBuilder currentPath = new StringBuilder();

    private uint gradientIdCounter = 1;

    private Color? currentFillColor = Colors.Black;
    private string? currentFillGradientId;

    private Color? currentStrokeColor = Colors.Black;
    private string? currentStrokeGradientId;

    private FillRule currentFillRule = FillRule.NonZero;
    private bool hasOpenPath = false;

    private Point currentPoint = (0, 0);

    private AffineTransform transform = AffineTransform.Identity;

    private List<nfloat> lineDashSegments = new List<NFloat>();
    private string? currentClipPathId;
    private int clipPathCounter = 0;

    private readonly Stack<SvgGroup> groupStack = new();

    private readonly Stack<DrawingState> stateStack = new();

    public nfloat GlobalAlpha { get; set; } = 1f;
    public LineCap LineCap { get; set; } = LineCap.Butt;
    public LineJoin LineJoin { get; set; } = LineJoin.Miter;
    public nfloat LineWidth { get; set; } = 1f;
    public nfloat MiterLimit { get; set; } = 10f;
    public nfloat LineDashOffset { get; set; } = 0f;
    public TextAlign TextAlign { get; set; } = TextAlign.Start;
    public TextBaseline TextBaseline { get; set; } = TextBaseline.Alphabetic;

    private Catalog catalog;
    private SvgFontResolver resolver;
    private readonly HashSet<FontFace> emittedFontFaces = new(); // to avoid duplicate @font-face

    private NumericFormat numericFormat;

    // Holds resolved font emission mode per FontFace to avoid recomputing.
    private readonly Dictionary<FontFace, Resolved> fontModeCache = new();

    private FontSnapshot font = new FontSnapshot();

    public SvgDrawingContext(Size canvasSize, Stream output, IEnumerable<Uri> fontUris, SvgFontResolver? resolver = null, NumericFormat? numericFormat = null, bool keepOpen = false)
        : this(canvasSize, output, new Catalog(fontUris), resolver, numericFormat, keepOpen)
    {
    }

    public SvgDrawingContext(Size canvasSize, Stream output, bool keepOpen = false)
        : this(canvasSize, output, (Catalog?)null, (SvgFontResolver?)null, (NumericFormat?)null, keepOpen)
    {
    }

    public SvgDrawingContext(Size canvasSize, Stream output, Catalog? catalog, SvgFontResolver? resolver = null, NumericFormat? numericFormat = null, bool keepOpen = false)
    {
        this.catalog = catalog ?? new Catalog();
        this.resolver = resolver ?? SvgFontResolver.Default;
        this.numericFormat = numericFormat ?? NumericFormat.Default;

        this.stream = output ?? throw new ArgumentNullException(nameof(output));

        this.keepOpen = keepOpen;
        this.canvasSize = canvasSize;

        this.styleBuffer = new();
        this.defsBuffer = new();
        this.bodyBuffer = new();

        this.styleWriter = new StreamWriter(styleBuffer, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
        this.defsWriter = new StreamWriter(defsBuffer, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
        this.bodyWriter = new StreamWriter(bodyBuffer, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;

            PopGroupsUntilDepth(0);

            styleWriter.Flush();
            defsWriter.Flush();
            bodyWriter.Flush();

            // Rewind both buffers to read from the start
            styleBuffer.Position = 0;
            defsBuffer.Position = 0;
            bodyBuffer.Position = 0;

            using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);

            // Write SVG header
            writer.WriteLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(canvasSize.Width)}\" height=\"{F(canvasSize.Height)}\" viewBox=\"0 0 {F(canvasSize.Width)} {F(canvasSize.Height)}\">");
            writer.Flush();

            // Write <defs> if any gradients/clips were defined
            if (styleBuffer.Length > 0)
            {
                writer.WriteLine("  <style>");
                writer.Flush();
                styleBuffer.CopyTo(writer.BaseStream);
                writer.Flush();
                writer.WriteLine("  </style>");
                writer.Flush();
            }

            if (defsBuffer.Length > 0)
            {
                writer.WriteLine("  <defs>");
                writer.Flush();
                defsBuffer.CopyTo(writer.BaseStream);
                writer.Flush();
                writer.WriteLine("  </defs>");
                writer.Flush();
            }

            stateStack.Clear();

            // Write all shape paths and fills
            bodyBuffer.CopyTo(writer.BaseStream);
            writer.Flush();

            // Close the SVG
            writer.WriteLine("</svg>");
            writer.Flush();

            if (!keepOpen)
                writer.Dispose();
        }
    }

    void IStateContext.Save()
    {
        stateStack.Push(new DrawingState
        {
            FillColor = currentFillColor,
            FillGradientId = currentFillGradientId,
            StrokeColor = currentStrokeColor,
            StrokeGradientId = currentStrokeGradientId,
            LineWidth = LineWidth,
            LineCap = LineCap,
            LineJoin = LineJoin,
            MiterLimit = MiterLimit,
            GlobalAlpha = GlobalAlpha,
            LineDashOffset = LineDashOffset,
            LineDashSegments = lineDashSegments.ToArray(),
            ClipPathId = currentClipPathId,
            HasOpenPath = hasOpenPath,
            CurrentPath = currentPath.ToString(),
            GroupDepth = groupStack.Count
        });
    }

    void IStateContext.Restore()
    {
        if (stateStack.Count == 0)
            throw new InvalidOperationException("No saved state to restore.");

        var state = stateStack.Pop();

        this.PopGroupsUntilDepth(state.GroupDepth);

        currentFillColor = state.FillColor;
        currentFillGradientId = state.FillGradientId;
        currentStrokeColor = state.StrokeColor;
        currentStrokeGradientId = state.StrokeGradientId;
        LineWidth = state.LineWidth;
        LineCap = state.LineCap;
        LineJoin = state.LineJoin;
        MiterLimit = state.MiterLimit;
        GlobalAlpha = state.GlobalAlpha;
        LineDashOffset = state.LineDashOffset;

        lineDashSegments.Clear();
        lineDashSegments.AddRange(state.LineDashSegments.AsSpan());

        currentClipPathId = state.ClipPathId;

        currentPath.Clear();
        currentPath.Append(state.CurrentPath);
        hasOpenPath = state.HasOpenPath;
    }

    void IPenContext.SetLineDash(ReadOnlySpan<nfloat> segments)
    {
        if (segments.Length == 0)
        {
            lineDashSegments.Clear();
            return;
        }

        lineDashSegments.AddRange(segments);
    }

    void IPenContext.SetStroke(Color color)
    {
        currentStrokeColor = color;
        currentStrokeGradientId = null; // clear gradient
    }

    void IPenContext.SetStroke(LinearGradient gradient)
    {
        currentStrokeColor = null;
        currentStrokeGradientId = $"grad{gradientIdCounter++}";

        defsWriter.WriteLine(
            $"    <linearGradient id=\"{currentStrokeGradientId}\" x1=\"{F(gradient.StartPoint.X)}\" y1=\"{F(gradient.StartPoint.Y)}\" x2=\"{F(gradient.EndPoint.X)}\" y2=\"{F(gradient.EndPoint.Y)}\" gradientUnits=\"userSpaceOnUse\">");

        foreach (var stop in gradient.GradientStops)
        {
            var color = ToSvgColor(stop.Color, out var opacity);
            var offset = $"{stop.Offset * 100:0.#}%";

            defsWriter.Write($"      <stop offset=\"{offset}\" stop-color=\"{color}\"");
            if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
                defsWriter.Write($" stop-opacity=\"{F(opacity.Value)}\"");
            defsWriter.WriteLine(" />");
        }

        defsWriter.WriteLine("    </linearGradient>");
    }

    void IPenContext.SetStroke(RadialGradient radial)
    {
        currentStrokeColor = null;
        currentStrokeGradientId = $"grad{gradientIdCounter++}";

        defsWriter.WriteLine(
            $"    <radialGradient id=\"{currentStrokeGradientId}\" cx=\"{F(radial.EndCenter.X)}\" cy=\"{F(radial.EndCenter.Y)}\" r=\"{F(radial.EndRadius)}\" fx=\"{F(radial.StartCenter.X)}\" fy=\"{F(radial.StartCenter.Y)}\" fr=\"{F(radial.StartRadius)}\" gradientUnits=\"userSpaceOnUse\">");

        foreach (var stop in radial.GradientStops)
        {
            var color = ToSvgColor(stop.Color, out var opacity);
            var offset = $"{stop.Offset * 100:0.#}%";

            defsWriter.Write($"      <stop offset=\"{offset}\" stop-color=\"{color}\"");
            if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
                defsWriter.Write($" stop-opacity=\"{F(opacity.Value)}\"");
            defsWriter.WriteLine(" />");
        }

        defsWriter.WriteLine("    </radialGradient>");
    }

    void IPenContext.SetFill(Color color)
    {
        currentFillColor = color;
        currentFillGradientId = null; // clear previous gradient
    }

    void IPenContext.SetFill(LinearGradient gradient)
    {
        currentFillColor = null;
        currentFillGradientId = $"grad{gradientIdCounter++}";

        defsWriter.WriteLine(
            $"    <linearGradient id=\"{currentFillGradientId}\" x1=\"{F(gradient.StartPoint.X)}\" y1=\"{F(gradient.StartPoint.Y)}\" x2=\"{F(gradient.EndPoint.X)}\" y2=\"{F(gradient.EndPoint.Y)}\" gradientUnits=\"userSpaceOnUse\">");

        foreach (var stop in gradient.GradientStops)
        {
            var color = ToSvgColor(stop.Color, out var opacity);
            var offset = $"{stop.Offset * 100:0.#}%";

            defsWriter.Write($"      <stop offset=\"{offset}\" stop-color=\"{color}\"");
            if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
                defsWriter.Write($" stop-opacity=\"{F(opacity.Value)}\"");
            defsWriter.WriteLine(" />");
        }

        defsWriter.WriteLine("    </linearGradient>");
    }

    void IPenContext.SetFill(RadialGradient radial)
    {
        currentFillColor = null;
        currentFillGradientId = $"grad{gradientIdCounter++}";

        defsWriter.WriteLine(
            $"    <radialGradient id=\"{currentFillGradientId}\" cx=\"{F(radial.EndCenter.X)}\" cy=\"{F(radial.EndCenter.Y)}\" r=\"{F(radial.EndRadius)}\" fx=\"{F(radial.StartCenter.X)}\" fy=\"{F(radial.StartCenter.Y)}\" fr=\"{F(radial.StartRadius)}\" gradientUnits=\"userSpaceOnUse\">");

        foreach (var stop in radial.GradientStops)
        {
            var color = ToSvgColor(stop.Color, out var opacity);
            var offset = $"{stop.Offset * 100:0.#}%";

            defsWriter.Write($"      <stop offset=\"{offset}\" stop-color=\"{color}\"");
            if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
                defsWriter.Write($" stop-opacity=\"{F(opacity.Value)}\"");
            defsWriter.WriteLine(" />");
        }

        defsWriter.WriteLine("    </radialGradient>");
    }

    void IImageDrawingContext.DrawImage(IImage image, Rect dest) =>
        throw new NotImplementedException("SVG context does not support DrawImage.");

    void IImageDrawingContext.DrawImage(IImage image, Rect dest, nfloat opacity) =>
        throw new NotImplementedException("SVG context does not support DrawImage.");

    void IImageDrawingContext.DrawImage(IImage image, Rect source, Rect dest, nfloat opacity) =>
        throw new NotImplementedException("SVG context does not support DrawImage.");

    void IPathBuilder.BeginPath()
    {
        currentPath.Clear();
        hasOpenPath = false;
    }

    void IGlyphPathBuilder.MoveTo(Point to)
    {
        EnsureClosedSubpath();
        currentPath.Append($"M {F(to.X)} {F(to.Y)} ");
        hasOpenPath = true;
        currentPoint = to;
    }

    void IGlyphPathBuilder.LineTo(Point to)
    {
        currentPath?.Append($"L {F(to.X)} {F(to.Y)} ");
        currentPoint = to;
    }

    void IGlyphPathBuilder.CurveTo(Point control, Point to)
    {
        currentPath?.Append($"Q {F(control.X)} {F(control.Y)}, {F(to.X)} {F(to.Y)} ");
        currentPoint = to;
    }

    void IGlyphPathBuilder.ClosePath()
    {
        if (hasOpenPath && currentPath.Length != 0)
        {
            currentPath.Append("Z ");
            hasOpenPath = false;
        }
    }

    void IPathBuilder.Rect(Rect rect)
    {
        EnsureClosedSubpath();

        currentPath.Append($"M {F(rect.X)} {F(rect.Y)} ");
        currentPath.Append($"H {F(rect.X + rect.Width)} ");
        currentPath.Append($"V {F(rect.Y + rect.Height)} ");
        currentPath.Append($"H {F(rect.X)} ");
        currentPath.Append("Z ");

        hasOpenPath = false;

        currentPoint = rect.BottomRight;
    }

    void IPathBuilder.RoundRect(Rect rect, nfloat radius)
    {
        EnsureClosedSubpath();

        var x = rect.X;
        var y = rect.Y;
        var w = rect.Width;
        var h = rect.Height;
        var r = nfloat.Min(radius, nfloat.Min(w / 2, h / 2));

        currentPath.Append($"M {F(x + r)} {F(y)} ");
        currentPath.Append($"H {F(x + w - r)} ");
        currentPath.Append($"A {F(r)} {F(r)} 0 0 1 {F(x + w)} {F(y + r)} ");
        currentPath.Append($"V {F(y + h - r)} ");
        currentPath.Append($"A {F(r)} {F(r)} 0 0 1 {F(x + w - r)} {F(y + h)} ");
        currentPath.Append($"H {F(x + r)} ");
        currentPath.Append($"A {F(r)} {F(r)} 0 0 1 {F(x)} {F(y + h - r)} ");
        currentPath.Append($"V {F(y + r)} ");
        currentPath.Append($"A {F(r)} {F(r)} 0 0 1 {F(x + r)} {F(y)} ");
        currentPath.Append("Z ");

        hasOpenPath = false;

        currentPoint = (x + r, y);
    }

    void IPathDrawing.Fill(FillRule rule)
    {
        if (currentPath.Length == 0)
            return;

        string fill;
        nfloat? opacity = null;

        if (currentFillGradientId != null)
        {
            fill = $"url(#{currentFillGradientId})";
        }
        else if (currentFillColor.HasValue)
        {
            fill = ToSvgColor(currentFillColor.Value, out opacity);
        }
        else
        {
            return;
        }

        string fillRule = rule == FillRule.EvenOdd ? "evenodd" : "nonzero";

        Ident();
        bodyWriter.Write($"<path d=\"{currentPath}\" fill=\"{fill}\" fill-rule=\"{fillRule}\"");

        if (opacity.HasValue)
            bodyWriter.Write($" fill-opacity=\"{F(opacity.Value)}\"");

        bodyWriter.WriteLine(" />");
    }

    void IPathDrawing.Stroke()
    {
        if (currentPath.Length == 0)
            return;

        string stroke;
        nfloat? opacity = null;

        if (currentStrokeGradientId != null)
            stroke = $"url(#{currentStrokeGradientId})";
        else if (currentStrokeColor.HasValue)
            stroke = ToSvgColor(currentStrokeColor.Value, out opacity);
        else
            return;

        Ident();
        bodyWriter.Write($"<path d=\"{currentPath}\" fill=\"none\"");
        WriteStrokeAttributes(stroke, opacity);
        bodyWriter.WriteLine(" />");
    }

    void IPathClipping.Clip()
    {
        if (currentPath.Length == 0)
            return;

        // Generate a unique ID
        currentClipPathId = $"clip{clipPathCounter++}";

        defsWriter.WriteLine($"    <clipPath id=\"{currentClipPathId}\">");
        defsWriter.WriteLine($"      <path d=\"{currentPath}\" clip-rule=\"nonzero\" />");
        defsWriter.WriteLine($"    </clipPath>");

        currentPath.Clear();
        hasOpenPath = false;

        PushGroup(null, currentClipPathId);
    }

    void IRectDrawingContext.StrokeRect(Rect rect)
    {
        string stroke;
        nfloat? opacity = null;

        if (currentStrokeGradientId != null)
            stroke = $"url(#{currentStrokeGradientId})";
        else if (currentStrokeColor.HasValue)
            stroke = ToSvgColor(currentStrokeColor.Value, out opacity);
        else
            return;

        Ident();
        bodyWriter.Write($"<rect x=\"{F(rect.X)}\" y=\"{F(rect.Y)}\" width=\"{F(rect.Width)}\" height=\"{F(rect.Height)}\" fill=\"none\"");
        WriteStrokeAttributes(stroke, opacity);
        bodyWriter.WriteLine(" />");
    }

    void IRectDrawingContext.FillRect(Rect rect)
    {
        string fill;
        nfloat? opacity = null;

        if (currentFillGradientId != null)
            fill = $"url(#{currentFillGradientId})";
        else if (currentFillColor.HasValue)
            fill = ToSvgColor(currentFillColor.Value, out opacity);
        else
            return; // Nothing to fill with

        Ident();
        bodyWriter.Write($"<rect x=\"{F(rect.X)}\" y=\"{F(rect.Y)}\" width=\"{F(rect.Width)}\" height=\"{F(rect.Height)}\" fill=\"{fill}\"");

        if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
            bodyWriter.Write($" fill-opacity=\"{F(opacity.Value)}\"");

        bodyWriter.WriteLine(" />");
    }

    void ITextDrawingContext.FillText(string text, Point pos)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!currentFillColor.HasValue)
            return; // Only support solid fill for now

        var fill = ToSvgColor(currentFillColor.Value, out var opacity);

        // Emit @font-face if needed
        if (font.FontFamily.Count > 0)
        {
            EmitFontFaceIfNeeded(new (
                family: font.FontFamily[0],
                weight: font.FontWeight,
                style: font.FontStyle,
                stretch: font.FontStretch
            ));
        }

        Ident();
        bodyWriter.Write($"<text x=\"{F(pos.X)}\" y=\"{F(pos.Y)}\" fill=\"{fill}\"");

        // Horizontal alignment
        var anchor = TextAlign switch
        {
            TextAlign.Center => "middle",
            TextAlign.Right => "end",
            _ => "start"
        };
        bodyWriter.Write($" text-anchor=\"{anchor}\"");

        // Vertical alignment
        var baseline = TextBaseline switch
        {
            TextBaseline.Top => "text-before-edge",
            TextBaseline.Middle => "central",
            TextBaseline.Bottom => "text-after-edge",
            TextBaseline.Hanging => "hanging",
            TextBaseline.Alphabetic => "alphabetic",
            TextBaseline.Ideographic => "ideographic",
            _ => "alphabetic"
        };
        bodyWriter.Write($" dominant-baseline=\"{baseline}\"");

        if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
            bodyWriter.Write($" fill-opacity=\"{F(opacity.Value)}\"");

        // Font
        bodyWriter.Write($" font-size=\"{F(font.FontSize)}\"");

        if (font.FontFamily.Count > 0)
        {
            var family = string.Join(",", font.FontFamily.Select(name => $"\"{name}\""));
            bodyWriter.Write($" font-family={family}");
        }

        if (font.FontWeight != FontWeight.Normal)
            bodyWriter.Write($" font-weight=\"{font.FontWeight}\"");

        if (font.FontStyle.IsItalic)
            bodyWriter.Write($" font-style=\"italic\"");

        bodyWriter.Write(" xml:space=\"preserve\">");
        bodyWriter.Write(System.Security.SecurityElement.Escape(text));
        bodyWriter.WriteLine("</text>");
    }

    private void EmitFontFaceIfNeeded(FontFace fontFace)
    {
        var ttf = this.catalog.FontForFace(fontFace);
        if (ttf is null)
            return;

        var sourceUri = ttf.SourceUri;

        if (!fontModeCache.TryGetValue(fontFace, out var resolved))
        {
            resolved = this.resolver.Resolve(fontFace, sourceUri);
            fontModeCache[fontFace] = resolved;
        }

        if (emittedFontFaces.Contains(fontFace))
            return;

        emittedFontFaces.Add(fontFace);

        if (resolved.Mode == SvgFontMode.WebLink && resolved.WebUri is not null)
        {
            styleWriter.WriteLine($"    @font-face {{ font-family: '{fontFace.Family}'; src: url('{resolved.WebUri}'); }}");
        }
        else if (resolved.Mode == SvgFontMode.Embedded && sourceUri is not null)
        {
            var data = catalog.LoadFromUri(sourceUri);
            var base64 = Convert.ToBase64String(data.Span);
            styleWriter.WriteLine($"    @font-face {{ font-family: '{fontFace.Family}'; src: url(data:font/ttf;base64,{base64}); }}");
        }
    }

    TextMetrics ITextMeasureContext.MeasureText(string text)
    {
        Xui.Core.Canvas.Font font = this.font.Font;
        TextMetrics textMetrics = this.catalog.MeasureText(in font, text, this.TextAlign, this.TextBaseline);
        return textMetrics;
    }

    void ITextMeasureContext.SetFont(Xui.Core.Canvas.Font font) => this.font.Font = font;

    void ITransformContext.Translate(Vector vector) =>
        PushGroup(AffineTransform.Translate(vector));

    void ITransformContext.Scale(Vector vector) =>
        PushGroup(AffineTransform.Scale(vector));

    void ITransformContext.Rotate(nfloat angle) =>
        PushGroup(AffineTransform.Rotate(angle));

    void ITransformContext.SetTransform(AffineTransform newTransform) =>
        PushGroup(transform.Inverse * newTransform);

    void ITransformContext.Transform(AffineTransform matrix) =>
        PushGroup(matrix);

    private static string ToSvgColor(Color color, out nfloat? opacity)
    {
        byte r = (byte)(color.Red * 255);
        byte g = (byte)(color.Green * 255);
        byte b = (byte)(color.Blue * 255);

        opacity = color.Alpha < 1f ? color.Alpha : null;
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private void EnsureClosedSubpath()
    {
        if (hasOpenPath && currentPath.Length == 0)
        {
            currentPath.Append("Z ");
            hasOpenPath = false;
        }
    }

    void IPathBuilder.CurveTo(Point cp1, Point cp2, Point to)
    {
        currentPath.Append($"C {F(cp1.X)} {F(cp1.Y)}, {F(cp2.X)} {F(cp2.Y)}, {F(to.X)} {F(to.Y)} ");
        currentPoint = to;
    }

    void IPathBuilder.Arc(Point center, nfloat radius, nfloat startAngle, nfloat endAngle, Winding winding)
    {
        var arc = new Arc(center, radius, radius, 0f, startAngle, endAngle, winding);
        var (arc1, arc2) = arc.ToEndpointArcs();

        AppendEndpointArcToPath(arc1);

        if (arc2 is ArcEndpoint second)
            AppendEndpointArcToPath(second);
    }

    private void AppendEndpointArcToPath(ArcEndpoint arc)
    {
        int largeArcFlag = arc.LargeArc ? 1 : 0;
        int sweepFlag = arc.Winding == Winding.ClockWise ? 1 : 0;

        // If the path is not open yet, move to start
        if (!hasOpenPath)
        {
            currentPath.Append($"M {F(arc.Start.X)} {F(arc.Start.Y)} ");
            hasOpenPath = true;
        }

        currentPath.Append($"A {F(arc.RadiusX)} {F(arc.RadiusY)} {F(arc.Rotation * 180 / nfloat.Pi)} {F(largeArcFlag)} {F(sweepFlag)} {F(arc.End.X)} {F(arc.End.Y)} ");

        currentPoint = arc.End;
    }

    void IPathBuilder.ArcTo(Point cp1, Point cp2, nfloat radius)
    {
        // If radius is zero or current point equals cp1, fallback to lineTo
        if (radius <= 0f || !hasOpenPath)
        {
            currentPath.Append($"L {F(cp1.X)} {F(cp1.Y)} ");
            return;
        }

        var p0 = this.currentPoint;
        var p1 = cp1;
        var p2 = cp2;

        var v0 = (p0 - p1).Normalized;
        var v1 = (p2 - p1).Normalized;

        var dot = Vector.Dot(v0, v1);
        if (dot >= 1.0f - 1e-6f)
        {
            // Points are collinear → straight line
            currentPath.Append($"L {F(cp1.X)} {F(cp1.Y)} ");
            return;
        }

        // Compute angle between v0 and v1
        var angle = nfloat.Acos(dot / (v0.Magnitude * v1.Magnitude));
        var tanHalfAngle = nfloat.Tan(angle / 2f);

        // Length from p1 along both directions to the arc tangents
        var dist = radius / tanHalfAngle;

        var start = p1 + v0 * dist;
        var end = p1 + v1 * dist;

        // Direction of arc: counterclockwise if cross product is positive
        bool sweep = Vector.Cross(v0, v1) < 0;

        // Move to start of arc if needed
        currentPath.Append($"L {F(start.X)} {F(start.Y)} ");

        // SVG uses endpoint arc, radius x = y, rotation = 0
        int largeArc = 0;
        int sweepFlag = sweep ? 1 : 0;

        currentPath.Append($"A {F(radius)} {F(radius)} 0 {largeArc} {sweepFlag} {F(end.X)} {F(end.Y)} ");
        hasOpenPath = true;

        currentPoint = end;
    }

    void IPathBuilder.Ellipse(Point center, nfloat radiusX, nfloat radiusY, nfloat rotation, nfloat startAngle, nfloat endAngle, Winding winding)
    {
        var arc = new Arc(center, radiusX, radiusY, rotation, startAngle, endAngle, winding);
        var (arc1, arc2) = arc.ToEndpointArcs();

        AppendEndpointArcToPath(arc1);

        if (arc2 is ArcEndpoint second)
            AppendEndpointArcToPath(second);
    }

    void IPathBuilder.RoundRect(Rect rect, CornerRadius radius)
    {
        EnsureClosedSubpath();

        nfloat x = rect.X;
        nfloat y = rect.Y;
        nfloat w = rect.Width;
        nfloat h = rect.Height;

        nfloat tl = nfloat.Min(radius.TopLeft, nfloat.Min(w, h) * (nfloat).5);
        nfloat tr = nfloat.Min(radius.TopRight, nfloat.Min(w, h) * (nfloat).5);
        nfloat br = nfloat.Min(radius.BottomRight, nfloat.Min(w, h) * (nfloat).5);
        nfloat bl = nfloat.Min(radius.BottomLeft, nfloat.Min(w, h) * (nfloat).5);

        // Top-left corner start
        currentPath.Append($"M {F(x + tl)} {F(y)} ");
        hasOpenPath = true;

        // Top edge
        currentPath.Append($"H {F(x + w - tr)} ");
        // Top-right corner arc
        AppendEndpointArcToPath(new ArcEndpoint(
            new Point(x + w - tr, y),
            new Point(x + w, y + tr),
            tr, tr, 0f, false, Winding.ClockWise));

        // Right edge
        currentPath.Append($"V {F(y + h - br)} ");
        // Bottom-right arc
        AppendEndpointArcToPath(new ArcEndpoint(
            new Point(x + w, y + h - br),
            new Point(x + w - br, y + h),
            br, br, 0f, false, Winding.ClockWise));

        // Bottom edge
        currentPath.Append($"H {F(x + bl)} ");
        // Bottom-left arc
        AppendEndpointArcToPath(new ArcEndpoint(
            new Point(x + bl, y + h),
            new Point(x, y + h - bl),
            bl, bl, 0f, false, Winding.ClockWise));

        // Left edge
        currentPath.Append($"V {F(y + tl)} ");
        // Top-left arc
        AppendEndpointArcToPath(new ArcEndpoint(
            new Point(x, y + tl),
            new Point(x + tl, y),
            tl, tl, 0f, false, Winding.ClockWise));

        currentPath.Append("Z ");
        hasOpenPath = false;

        currentPoint = (x + tl, y);
    }

    private string F(nfloat value) => numericFormat.Format(value);

    private void PushGroup(AffineTransform? localTransform = null, string? clipPathId = null)
    {
        // Compute global transform
        var global = localTransform.HasValue
            ? transform * localTransform.Value
            : transform;

        Ident();
        bodyWriter.Write("<g");

        if (clipPathId != null)
            bodyWriter.Write($" clip-path=\"url(#{clipPathId})\"");

        if (localTransform.HasValue && !localTransform.Value.IsIdentity)
            bodyWriter.Write($" transform=\"{FormatTransform(localTransform.Value)}\"");

        bodyWriter.WriteLine(">");

        // Push to stack
        groupStack.Push(new SvgGroup(localTransform, global, clipPathId));
        transform = global;
    }

    private void PopGroupsUntilDepth(int targetDepth)
    {
        while (groupStack.Count > targetDepth)
        {
            groupStack.Pop();
            Ident();
            bodyWriter.WriteLine("</g>");
        }

        transform = groupStack.Count == 0 ? AffineTransform.Identity : groupStack.Peek().GlobalTransform;
    }

    private void Ident()
    {
        for (var i = 0; i < this.groupStack.Count + 1; i++)
        {
            bodyWriter.Write("  ");
        }
    }

    private string FormatTransform(AffineTransform t)
    {
        if (t.B == 0 && t.C == 0)
        {
            if (t.A != 1 || t.D != 1)
            {
                if (t.Tx != 0 || t.Ty != 0)
                    return $"translate({F(t.Tx)},{F(t.Ty)}) scale({F(t.A)},{F(t.D)})";
                return $"scale({F(t.A)},{F(t.D)})";
            }
            return $"translate({F(t.Tx)},{F(t.Ty)})";
        }

        return $"matrix({F(t.A)},{F(t.B)},{F(t.C)},{F(t.D)},{F(t.Tx)},{F(t.Ty)})";
    }

    private void WriteStrokeAttributes(string stroke, nfloat? opacity)
    {
        bodyWriter.Write($" stroke=\"{stroke}\"");

        if (LineWidth != 1f)
            bodyWriter.Write($" stroke-width=\"{F(LineWidth)}\"");

        if (LineCap != LineCap.Butt)
            bodyWriter.Write($" stroke-linecap=\"{LineCap.ToString().ToLowerInvariant()}\"");

        if (LineJoin != LineJoin.Miter)
            bodyWriter.Write($" stroke-linejoin=\"{LineJoin.ToString().ToLowerInvariant()}\"");

        if (LineJoin == LineJoin.Miter && MiterLimit != 10f)
            bodyWriter.Write($" stroke-miterlimit=\"{F(MiterLimit)}\"");

        if (lineDashSegments.Count > 0)
        {
            var dashArray = string.Join(",", lineDashSegments.Select(F));
            bodyWriter.Write($" stroke-dasharray=\"{dashArray}\"");

            if (LineDashOffset != 0f)
                bodyWriter.Write($" stroke-dashoffset=\"{F(LineDashOffset)}\"");
        }

        if (opacity.HasValue && opacity.Value > 0f && opacity.Value < 1f)
            bodyWriter.Write($" stroke-opacity=\"{F(opacity.Value)}\"");
    }

    private readonly struct SvgGroup
    {
        public AffineTransform? LocalTransform { get; }
        public AffineTransform GlobalTransform { get; }
        public string? ClipPathId { get; }

        public SvgGroup(AffineTransform? localTransform, AffineTransform globalTransform, string? clipPathId)
        {
            LocalTransform = localTransform;
            GlobalTransform = globalTransform;
            ClipPathId = clipPathId;
        }

        public bool HasTransform => LocalTransform.HasValue && !LocalTransform.Value.IsIdentity;
        public bool HasClip => ClipPathId != null;
    }

    private struct DrawingState
    {
        public Color? FillColor;
        public string? FillGradientId;

        public Color? StrokeColor;
        public string? StrokeGradientId;

        public nfloat LineWidth;
        public LineCap LineCap;
        public LineJoin LineJoin;
        public nfloat MiterLimit;
        public nfloat GlobalAlpha;

        public nfloat LineDashOffset;
        public nfloat[] LineDashSegments;

        public string? ClipPathId;
        public bool HasOpenPath;
        public string? CurrentPath;

        public int GroupDepth;
    }

    /// <summary>
    /// Immutable snapshot of a <see cref="Font"/> for storage and comparison.
    /// </summary>
    private struct FontSnapshot
    {
        public readonly List<string> FontFamily = new List<string>();

        public nfloat FontSize;
        public FontWeight FontWeight;
        public FontStyle FontStyle;
        public FontStretch FontStretch;
        public nfloat LineHeight;

        public FontSnapshot()
        {
        }

        public Xui.Core.Canvas.Font Font
        {
            readonly get
            {
                return new Xui.Core.Canvas.Font(FontSize,
                    CollectionsMarshal.AsSpan(FontFamily),
                    FontWeight,
                    FontStyle,
                    FontStretch,
                    LineHeight);
            }

            set
            {
                this.FontSize = value.FontSize;
                FontWeight = value.FontWeight;
                FontStyle = value.FontStyle;
                FontStretch = value.FontStretch;
                LineHeight = value.LineHeight;

                FontFamily.Clear();
                var span = value.FontFamily;
                for (int i = 0; i < span.Length; i++)
                    FontFamily.Add(span[i]);
            }
        }
    }

    public abstract class NumericFormat
    {
        public abstract string Format(nfloat number);

        public static NumericFormat Default { get; } = new AngleFriendlyFormat();

        private sealed class AngleFriendlyFormat : NumericFormat
        {
            public override string Format(nfloat value) =>
                value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Delegate-based resolver that provides how a specific font should be handled in an SVG.
    /// </summary>
    public class SvgFontResolver
    {
        public static readonly SvgFontResolver Default = new SvgFontResolver();

        public virtual Resolved Resolve(FontFace face, Uri? uri) => new Resolved(SvgFontMode.System, null);
    }

    public record struct Resolved (SvgFontMode Mode, Uri? WebUri);

    /// <summary>
    /// Determines how to emit fonts inside an SVG.
    /// </summary>
    public enum SvgFontMode
    {
        /// <summary>Assume the font is installed in the system; only use font-family="...".</summary>
        System,

        /// <summary>Emit a link to an external font using @font-face with a resolved URI.</summary>
        WebLink,

        /// <summary>Embed the font data as base64 using a data URI inside @font-face.</summary>
        Embedded
    }
}
