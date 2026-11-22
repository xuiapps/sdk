using System.Xml.Linq;

namespace Xui.Apps.TestApp.Pages.Floorplan;

// Presentation attributes only. The authoring build resolves CSS and variables;
// this value type carries SVG inheritance without a dictionary per element.
internal readonly record struct SvgPresentation(
    string Fill, string Stroke, string FillOpacity, string StrokeOpacity,
    string FillRule, string StrokeWidth, string LineCap, string LineJoin,
    string FontSize, string FontWeight, string TextAnchor, string Color, string PaintOrder)
{
    public static SvgPresentation Default => new("#000", "none", "1", "1", "nonzero", "1",
        "butt", "miter", "16", "400", "start", "#000", "normal");

    public SvgPresentation Resolve(XElement element) => new(
        Read(element, "fill", Fill), Read(element, "stroke", Stroke),
        Read(element, "fill-opacity", FillOpacity), Read(element, "stroke-opacity", StrokeOpacity),
        Read(element, "fill-rule", FillRule), Read(element, "stroke-width", StrokeWidth),
        Read(element, "stroke-linecap", LineCap), Read(element, "stroke-linejoin", LineJoin),
        Read(element, "font-size", FontSize), Read(element, "font-weight", FontWeight),
        Read(element, "text-anchor", TextAnchor), Read(element, "color", Color),
        Read(element, "paint-order", PaintOrder));

    private static string Read(XElement element, XName name, string inherited) =>
        element.Attribute(name)?.Value is { } value && value != "inherit" ? value : inherited;
}
