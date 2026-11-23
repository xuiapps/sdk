using System.Runtime.InteropServices;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Tessellate;
using static Xui.Runtime.IOS.CoreGraphics;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

public sealed partial class FloorplanDrawing
{
    private readonly record struct GeometryVertex(Point Point, Vector Normal, NFloat Coverage);
    private sealed record GeometryQuad(GeometryVertex[] Vertices, Sprite Color);

    private static List<GeometryQuad> CompileStructure(XElement resolved, int level, Dictionary<uint, Sprite> colors, List<Sprite> sprites)
    {
        var root = new XElement(resolved);
        foreach (var e in root.Descendants().Where(e => !e.Ancestors().Any(a => a.Name.LocalName == "defs")).ToArray())
        {
            if (e.Name.LocalName is not ("path" or "rect" or "line" or "circle" or "ellipse" or "polygon" or "polyline" or "use" or "text")) continue;
            var layer = e.AncestorsAndSelf().Select(a => a.Attribute("data-layer")?.Value).FirstOrDefault(v => v is not null);
            if (layer is not ("structure" or "underlay")) e.Remove();
        }
        var drawing = new Loader(root).Build();
        var result = new List<GeometryQuad>();
        using var space = CGColorSpaceRef.CreateDeviceRGB();
        var context = CGBitmapContextCreate(0, 4, 4, 8, 16, space, CGBitmapInfo.ByteOrder32Little | CGBitmapInfo.AlphaPremultipliedFirst);
        if (context == 0) throw new InvalidOperationException("Unable to allocate geometry compiler context.");
        try { Visit(drawing.levels[level], AffineTransform.Identity); }
        finally { CGContextRef.CGContextRelease(context); }
        return result;

        void Visit(Item[] items, AffineTransform parent)
        {
            foreach (var item in items)
            {
                var transform = parent * item.Transform;
                if (item.Symbol is { } symbol)
                {
                    // Built-in cabinets/lockers live in some structure groups but already
                    // have furniture atlas quads. Only door symbols belong in this mesh.
                    if (symbol.SourceId.Contains("door", StringComparison.Ordinal)) Visit(symbol.Items, transform);
                    continue;
                }
                if (item.Path is null || item.Alpha <= 0) continue;
                CGContextRef.CGContextBeginPath(context);
                item.Path.Visit(new GeometryPathSink(context));
                using var path = new CGPathRef(CGContextCopyPath(context));
                nint nativeSource = path;
                if (item.StrokeFirst) Stroke();
                if (item.Fill is { } fill) Emit(path, fill, item.Rule);
                if (!item.StrokeFirst) Stroke();

                void Stroke()
                {
                    if (item.Stroke is not { } stroke || item.Width <= 0) return;
                    using var outline = new CGPathRef(CGPathCreateCopyByStrokingPath(nativeSource, 0, item.Width, (int)item.Cap, (int)item.Join, 10));
                    Emit(outline, stroke, FillRule.NonZero);
                }
                void Emit(nint nativePath, Paint paint, FillRule rule)
                {
                    if (paint.Stops is not null) throw new NotSupportedException("Structure meshes currently require solid paints.");
                    var color = paint.Color; color.Alpha *= item.Alpha;
                    if (color.Alpha <= 0) return;
                    var swatch = Swatch(color);
                    var path = ReadNativePath(nativePath);
                    NFloat scale = NFloat.Max(NFloat.Sqrt(transform.A * transform.A + transform.B * transform.B), NFloat.Sqrt(transform.C * transform.C + transform.D * transform.D));
                    var tessellation = PathTesselator.Fill(path, rule, (NFloat).025 / NFloat.Max(scale, (NFloat).001));
                    // The scanline tessellator emits pairs of triangles forming non-overlapping trapezoids.
                    var triangles = tessellation.Polygons;
                    for (int i = 0; i < triangles.Count; i += 2)
                    {
                        var a = triangles[i]; var b = triangles[i + 1];
                        result.Add(new([Vertex(a.A), Vertex(a.C), Vertex(b.C), Vertex(a.B)], swatch));
                    }
                    var contours = tessellation.Contours.Select(c => c.Points.Select(p => transform * p).ToArray()).ToArray();
                    AddEdges(contours, rule, swatch, result);
                    GeometryVertex Vertex(Point p) => new(transform * p, default, 1);
                }
            }
        }
        Sprite Swatch(Color color)
        {
            byte a = Byte(color.Alpha), r = Byte(color.Red * color.Alpha), g = Byte(color.Green * color.Alpha), b = Byte(color.Blue * color.Alpha);
            uint key = (uint)(r << 24 | g << 16 | b << 8 | a);
            if (colors.TryGetValue(key, out var swatch)) return swatch;
            // A constant color cell stays constant through all five mip levels, including its center sample.
            byte[] pixels = new byte[64 * 64 * 4];
            for (int i = 0; i < pixels.Length; i += 4) { pixels[i] = r; pixels[i + 1] = g; pixels[i + 2] = b; pixels[i + 3] = a; }
            swatch = new("structure-color-" + key.ToString("X8"), -1, default, 64, 64, pixels);
            colors.Add(key, swatch); sprites.Add(swatch); return swatch;
        }
        static byte Byte(NFloat value) => (byte)NFloat.Clamp(NFloat.Round(value * 255), 0, 255);
    }

    private static void AddEdges(Point[][] contours, FillRule rule, Sprite color, List<GeometryQuad> output)
    {
        foreach (var original in contours)
        {
            var points = original.Where((p, i) => i == 0 || p != original[i - 1]).ToArray();
            int count = points.Length;
            if (count > 1 && points[0] == points[^1]) count--;
            if (count < 3) continue;
            var normals = new Vector[count];
            for (int i = 0; i < count; i++)
            {
                var a = points[i]; var b = points[(i + 1) % count];
                var edge = b - a; var normal = new Vector(-edge.Y, edge.X).Normalized;
                if (Inside(Point.Lerp(a, b, (NFloat).5) + normal * (NFloat).00001)) normal *= -1;
                normals[i] = normal;
            }
            var corners = new Vector[count];
            for (int i = 0; i < count; i++)
            {
                var previous = normals[(i + count - 1) % count]; var next = normals[i];
                var sum = previous + next;
                var denominator = 1 + Vector.Dot(previous, next);
                corners[i] = denominator > (NFloat).25 ? sum / denominator : next;
            }
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                output.Add(new([
                    new(points[i], default, 1), new(points[next], default, 1),
                    new(points[next], corners[next] * (NFloat).75, 0), new(points[i], corners[i] * (NFloat).75, 0)
                ], color));
            }
        }
        bool Inside(Point p)
        {
            int winding = 0;
            foreach (var contour in contours)
            for (int i = 0; i < contour.Length; i++)
            {
                var a = contour[i]; var b = contour[(i + 1) % contour.Length];
                var cross = Vector.Cross(b - a, p - a);
                if (a.Y <= p.Y && b.Y > p.Y && cross > 0) winding++;
                else if (a.Y > p.Y && b.Y <= p.Y && cross < 0) winding--;
            }
            return rule == FillRule.EvenOdd ? winding % 2 != 0 : winding != 0;
        }
    }

    private const string GraphicsLibrary = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    [DllImport(GraphicsLibrary)] private static extern nint CGContextCopyPath(nint context);
    [DllImport(GraphicsLibrary)] private static extern nint CGPathCreateCopyByStrokingPath(nint path, nint transform, NFloat width, int cap, int join, NFloat miterLimit);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void PathApplier(nint info, nint element);
    [DllImport(GraphicsLibrary)] private static extern void CGPathApply(nint path, nint info, PathApplier applier);
    [StructLayout(LayoutKind.Sequential)] private struct NativePathElement { public int Type; public nint Points; }

    private static unsafe Path2D ReadNativePath(nint path)
    {
        var result = new Path2D();
        Exception? failure = null;
        CGPathApply(path, 0, (_, address) =>
        {
            if (failure is not null) return;
            try
            {
                var element = Marshal.PtrToStructure<NativePathElement>(address);
                var p = (Point*)element.Points;
                switch (element.Type)
                {
                    case 0: result.MoveTo(p[0]); break;
                    case 1: result.LineTo(p[0]); break;
                    case 2: result.CurveTo(p[0], p[1]); break;
                    case 3: result.CurveTo(p[0], p[1], p[2]); break;
                    case 4: result.ClosePath(); break;
                    default: throw new InvalidDataException("Unknown Quartz path element.");
                }
            }
            catch (Exception error) { failure = error; }
        });
        if (failure is not null) throw new InvalidDataException("Cannot read structure outline.", failure);
        result.ClosePath(); // SVG fills implicitly close the final subpath too.
        return result;
    }

    private sealed class GeometryPathSink(nint context) : IPathBuilder
    {
        public void BeginPath() => CGContextRef.CGContextBeginPath(context);
        public void MoveTo(Point p) => CGContextRef.CGContextMoveToPoint(context, p.X, p.Y);
        public void LineTo(Point p) => CGContextRef.CGContextAddLineToPoint(context, p.X, p.Y);
        public void ClosePath() => CGContextRef.CGContextClosePath(context);
        public void CurveTo(Point a, Point b) => CGContextRef.CGContextAddQuadCurveToPoint(context, a.X, a.Y, b.X, b.Y);
        public void CurveTo(Point a, Point b, Point c) => CGContextRef.CGContextAddCurveToPoint(context, a.X, a.Y, b.X, b.Y, c.X, c.Y);
        public void Arc(Point p, NFloat radius, NFloat start, NFloat end, Winding winding = Winding.ClockWise) => CGContextRef.CGContextAddArc(context, p.X, p.Y, radius, start, end, 1 - (int)winding);
        public void ArcTo(Point a, Point b, NFloat radius) => CGContextRef.CGContextAddArcToPoint(context, a.X, a.Y, b.X, b.Y, radius);
        public void Rect(Rect rect) => CGContextRef.CGContextAddRect(context, rect);
        public void RoundRect(Rect rect, NFloat radius)
        { using var path = CGPathRef.CreateWithRoundedRect(rect, radius); CGContextRef.CGContextAddPath(context, path); }
        public void RoundRect(Rect rect, CornerRadius radius) => throw new NotSupportedException("Per-corner structure radii are not authored.");
        public void Ellipse(Point p, NFloat rx, NFloat ry, NFloat rotation, NFloat start, NFloat end, Winding winding = Winding.ClockWise)
        {
            CGContextRef.CGContextSaveGState(context);
            CGContextRef.CGContextTranslateCTM(context, p.X, p.Y); CGContextRef.CGContextRotateCTM(context, rotation);
            CGContextRef.CGContextScaleCTM(context, rx, ry);
            CGContextRef.CGContextAddArc(context, 0, 0, 1, start, end, 1 - (int)winding);
            CGContextRef.CGContextRestoreGState(context);
        }
    }
}
