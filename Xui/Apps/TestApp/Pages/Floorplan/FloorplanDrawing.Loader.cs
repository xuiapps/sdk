using System.Globalization;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

public sealed partial class FloorplanDrawing
{
    private static FloorplanDrawing LoadEmbedded()
    {
        using var stream = typeof(FloorplanDrawing).Assembly.GetManifestResourceStream("TestApp.Floorplan.Office.svg")
            ?? throw new InvalidOperationException("Missing embedded office SVG.");
        return Load(stream);
    }
    public static FloorplanDrawing Load(Stream stream) => new Loader(XDocument.Load(stream).Root!).Build();
    private sealed class Loader(XElement root)
    {
        private readonly Dictionary<string, XElement> ids = root.DescendantsAndSelf().Where(e => e.Attribute("id") != null).ToDictionary(e => e.Attribute("id")!.Value);
        private readonly Dictionary<XElement, Path2D> paths = new();
        private readonly Dictionary<XElement, Region> regions = new();
        private readonly HashSet<XElement> references = new();
        private readonly List<Item> output = new();
        private bool markersOnly;
        private int symbolDepth;
        private readonly Dictionary<(XElement, SvgPresentation, NFloat), SymbolDefinition> symbols = new();
        private readonly (XElement Element, string[] Attributes)[] variants = root.DescendantsAndSelf()
            .Where(e => e.Attribute("data-state-attrs") is not null)
            .Select(e => (e, e.Attribute("data-state-attrs")!.Value.Split(' '))).ToArray();
        private static readonly Rect Building = new(0, 120, 1000, 760);
        public FloorplanDrawing Build()
        {
            // Fail fast on stale authored input instead of silently drawing unstyled geometry.
            if (root.DescendantsAndSelf().Any(e => e.Name.LocalName == "style" || e.Attribute("style") is not null
                || e.Attributes().Any(a => a.Value.Contains("var(", StringComparison.Ordinal))))
                throw new NotSupportedException("Compile the floorplan SVG with preview.py export before loading it; CSS is not supported.");
            var levels = new Item[4][];
            for (int i = 0; i < 4; i++)
                levels[i] = Compile(i, false);
            var markers = Compile(0, true);
            return new(levels, markers, paths.Count);
        }
        private Item[] Compile(int level, bool markers)
        {
            markersOnly = markers;
            output.Clear();
            var prefix = $"data-l{level}w{(level == 0 ? 0 : 1)}-";
            foreach (var (element, attributes) in variants)
                foreach (var name in attributes)
                    element.SetAttributeValue(name, element.Attribute(prefix + name)?.Value
                        ?? throw new FormatException($"Missing floorplan state attribute {prefix + name}."));
            // Marker visibility is animated by FloorplanView, independently of furniture LoD.
            if (ids.TryGetValue("building-constellations", out var marker))
                marker.SetAttributeValue("opacity", "1");
            Walk(root, SvgPresentation.Default, AffineTransform.Identity, Building, 1, false);
            return output.ToArray();
        }
        private void Walk(XElement element, SvgPresentation inherited, AffineTransform transform, Rect bounds, NFloat alpha, bool insideMarkers, Region? region = null)
        {
            string tag = element.Name.LocalName;
            if (tag is "defs" or "style" or "title" or "desc")
                return;
            if (tag == "image")
                throw new NotSupportedException("Floorplan must contain vectors only.");
            bool markerRoot = element.Attribute("id")?.Value == "building-constellations";
            if (markerRoot && !markersOnly)
                return;
            insideMarkers |= markerRoot;
            if (element.Attribute("display")?.Value == "none") return;
            var style = inherited.Resolve(element);
            alpha *= Attr(element, "opacity", 1);
            transform *= Transform(element.Attribute("transform")?.Value);
            // Room bounds are building coordinates; symbol bounds must not replace them.
            if (element.Attribute("data-bounds") is { } box)
            {
                bounds = RectFrom(box.Value);
                if (!markersOnly && symbolDepth == 0)
                {
                    if (!regions.TryGetValue(element, out region))
                        regions[element] = region = new(element.Attribute("id")?.Value ?? "floorplan", bounds,
                            element.Attribute("data-kind")?.Value == "room");
                }
            }
            if (insideMarkers)
                bounds = Building; // Halos intentionally extend beyond room footprints.
            if (tag == "use")
            {
                string reference = element.Attribute("href")?.Value ?? throw new FormatException("Missing use reference.");
                if (!reference.StartsWith('#') || !ids.TryGetValue(reference[1..], out var target))
                    throw new FormatException($"Unknown SVG reference {reference}.");
                if (!references.Add(target))
                    throw new FormatException("Cyclic SVG symbol reference.");
                var m = AffineTransform.Translate((Attr(element, "x"), Attr(element, "y")));
                if (target.Name.LocalName == "symbol")
                {
                    var vb = RectFrom(target.Attribute("viewBox")!.Value);
                    var w = Attr(element, "width", vb.Width);
                    var h = Attr(element, "height", vb.Height);
                    var scale = NFloat.Min(w / vb.Width, h / vb.Height);
                    m *= AffineTransform.Translate(((w - vb.Width * scale) / 2, (h - vb.Height * scale) / 2))
                        * AffineTransform.Scale((scale, scale)) * AffineTransform.Translate((-vb.X, -vb.Y));
                }
                if (target.Name.LocalName == "symbol" && !insideMarkers && !markersOnly && symbolDepth == 0)
                {
                    // The key contains presentation values only, never placement IDs or geometry.
                    var signature = (target, style, alpha);
                    if (!symbols.TryGetValue(signature, out var symbol))
                    {
                        var start = output.Count;
                        var localBounds = RectFrom(target.Attribute("viewBox")!.Value).Expand(8);
                        symbolDepth++;
                        try { Walk(target, style, AffineTransform.Identity, localBounds, alpha, false); }
                        finally { symbolDepth--; }
                        var items = output.GetRange(start, output.Count - start).ToArray();
                        output.RemoveRange(start, output.Count - start);
                        symbols[signature] = symbol = new(items, localBounds, target.Attribute("data-source-id")?.Value ?? target.Attribute("id")!.Value);
                    }
                    output.Add(new Item(null, null, default, default, default, transform * m, bounds,
                        null, null, 0, default, default, default, 1, false, false, symbol, region));
                }
                else Walk(target, style, transform * m, bounds, alpha, insideMarkers, region);
                references.Remove(target);
            }
            else if (tag is "svg" or "g" or "symbol")
            {
                foreach (var child in element.Elements())
                    Walk(child, style, transform, bounds, alpha, insideMarkers, region);
            }
            else if (!markersOnly || insideMarkers)
            {
                if (tag is not ("path" or "rect" or "circle" or "ellipse" or "line" or "polyline" or "polygon" or "text"))
                    throw new NotSupportedException($"Unsupported floorplan element: {tag}");
                Path2D? path = null;
                if (tag != "text" && !paths.TryGetValue(element, out path))
                    paths[element] = path = Shape(element);
                var fill = PaintFor(style.Fill, Number(style.FillOpacity), element, style);
                var stroke = PaintFor(style.Stroke, Number(style.StrokeOpacity), element, style);
                var weight = style.FontWeight switch { "normal" => (NFloat)400, "bold" => 700, var value => Number(value) };
                var font = new Font(Number(style.FontSize), "Inter", new FontWeight(weight));
                var item = new Item(path, tag == "text" ? element.Value : null, (Attr(element, "x"), Attr(element, "y")), font,
                    style.TextAnchor == "middle" ? TextAlign.Center : style.TextAnchor == "end" ? TextAlign.Right : TextAlign.Left,
                    transform, bounds, fill, stroke, Number(style.StrokeWidth),
                    style.LineCap == "round" ? LineCap.Round : style.LineCap == "square" ? LineCap.Square : LineCap.Butt,
                    style.LineJoin == "round" ? LineJoin.Round : style.LineJoin == "bevel" ? LineJoin.Bevel : LineJoin.Miter,
                    style.FillRule == "evenodd" ? FillRule.EvenOdd : FillRule.NonZero, alpha,
                    style.PaintOrder.StartsWith("stroke", StringComparison.Ordinal), tag == "text" && !insideMarkers, Region: region);
                if (tag == "text" && element.HasElements)
                {
                    // The study uses explicitly positioned tspan lines, not inline text layout.
                    var position = item.Position;
                    foreach (var span in element.Elements())
                    {
                        if (span.Name.LocalName != "tspan")
                            throw new NotSupportedException("Expected positioned tspan.");
                        position = (Attr(span, "x", position.X) + Attr(span, "dx"), Attr(span, "y", position.Y) + Attr(span, "dy"));
                        output.Add(item with { Text = span.Value, Position = position });
                    }
                }
                else
                    output.Add(item);
            }
        }
        private Paint? PaintFor(string value, NFloat opacity, XElement shape, SvgPresentation style)
        {
            if (value == "none")
                return null;
            if (value == "currentColor")
                value = style.Color;
            if (!value.StartsWith("url(", StringComparison.Ordinal))
            { var color = ColorOf(value); color.Alpha *= opacity; return new(color); }
            var gradient = ids[value[5..^1]];
            var stops = gradient.Elements().Select(stop =>
            {
                var c = ColorOf(stop.Attribute("stop-color")?.Value ?? "#000");
                c.Alpha *= Attr(stop, "stop-opacity", 1) * opacity;
                return new GradientStop(Fraction(stop.Attribute("offset")?.Value ?? "0"), c);
            }).ToArray();
            var box = ShapeBounds(shape);
            if (gradient.Name.LocalName == "radialGradient")
                return new(default, stops, box.Center, default, NFloat.Max(box.Width, box.Height) / 2);
            NFloat x1 = Fraction(gradient.Attribute("x1")?.Value ?? "0"), y1 = Fraction(gradient.Attribute("y1")?.Value ?? "0");
            NFloat x2 = Fraction(gradient.Attribute("x2")?.Value ?? "100%"), y2 = Fraction(gradient.Attribute("y2")?.Value ?? "0");
            if (gradient.Attribute("gradientUnits")?.Value == "userSpaceOnUse")
                return new(default, stops, (x1, y1), (x2, y2));
            return new(default, stops, (box.X + x1 * box.Width, box.Y + y1 * box.Height), (box.X + x2 * box.Width, box.Y + y2 * box.Height));
        }
    }
    private static NFloat Number(string s) => (NFloat)double.Parse(s.Replace("px", ""), CultureInfo.InvariantCulture);
    private static NFloat Fraction(string s) => s.EndsWith('%') ? Number(s[..^1]) / 100 : Number(s);
    private static NFloat Attr(XElement e, string key, NFloat fallback = default) => e.Attribute(key) is { } a ? Number(a.Value) : fallback;
    private static Rect RectFrom(string s) { var r = new SvgPathReader(s); return new(r.Number(), r.Number(), r.Number(), r.Number()); }
    private static Color ColorOf(string s)
    {
        s = s.Trim();
        if (s == "white")
            return Colors.White;
        if (s == "black")
            return Colors.Black;
        if (!s.StartsWith('#'))
            throw new FormatException($"Unsupported SVG color {s}");
        s = s[1..];
        if (s.Length == 3)
            s = string.Concat(s.Select(c => new string(c, 2)));
        if (s.Length == 6)
            s += "ff";
        return new Color(uint.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }
    private static AffineTransform Transform(string? value)
    {
        var result = AffineTransform.Identity;
        if (value is null)
            return result;
        var transforms = new SvgTransformReader(value);
        while (transforms.MoveNext())
        {
            var r = new SvgPathReader(transforms.Arguments);
            var a = r.Number();
            switch (transforms.Name)
            {
                case "translate":
                    result *= AffineTransform.Translate((a, r.More ? r.Number() : 0));
                    break;
                case "scale":
                    result *= AffineTransform.Scale((a, r.More ? r.Number() : a));
                    break;
                case "rotate":
                    var rotation = AffineTransform.Rotate(a * NFloat.Pi / 180);
                    if (r.More)
                    { var p = new Vector(r.Number(), r.Number()); rotation = AffineTransform.Translate(p) * rotation * AffineTransform.Translate((-p.X, -p.Y)); }
                    result *= rotation;
                    break;
                case "matrix":
                    result *= new AffineTransform(a, r.Number(), r.Number(), r.Number(), r.Number(), r.Number());
                    break;
                default:
                    throw new NotSupportedException("Unsupported SVG transform.");
            }
        }
        return result;
    }
    private static Rect ShapeBounds(XElement e) => e.Name.LocalName is "circle" or "ellipse"
        ? new(Attr(e, "cx") - Attr(e, "rx", Attr(e, "r")), Attr(e, "cy") - Attr(e, "ry", Attr(e, "r")), Attr(e, "rx", Attr(e, "r")) * 2, Attr(e, "ry", Attr(e, "r")) * 2)
        : new(Attr(e, "x"), Attr(e, "y"), Attr(e, "width"), Attr(e, "height"));
    private static Path2D Shape(XElement e)
    {
        if (e.Name.LocalName == "path")
            return SvgPathReader.Parse(e.Attribute("d")!.Value);
        var p = new Path2D();
        var box = ShapeBounds(e);
        switch (e.Name.LocalName)
        {
            case "rect":
                var radius = Attr(e, "rx", Attr(e, "ry"));
                if (radius > 0)
                    p.RoundRect(box, NFloat.Min(radius, NFloat.Min(box.Width, box.Height) / 2));
                else
                    p.Rect(box);
                break;
            case "circle":
            case "ellipse":
                p.Ellipse(box.Center, box.Width / 2, box.Height / 2, 0, 0, 2 * NFloat.Pi);
                p.ClosePath();
                break;
            case "line":
                p.MoveTo((Attr(e, "x1"), Attr(e, "y1")));
                p.LineTo((Attr(e, "x2"), Attr(e, "y2")));
                break;
            case "polyline":
            case "polygon":
                var r = new SvgPathReader(e.Attribute("points")!.Value);
                p.MoveTo((r.Number(), r.Number()));
                while (r.More)
                    p.LineTo((r.Number(), r.Number()));
                if (e.Name.LocalName == "polygon")
                    p.ClosePath();
                break;
        }
        return p;
    }
}
