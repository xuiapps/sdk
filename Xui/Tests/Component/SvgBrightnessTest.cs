using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class SvgBrightnessTest
{
    private static readonly XNamespace Ns = "http://www.w3.org/2000/svg";
    private static readonly Rect Bounds = new(0, 0, 100, 100);

    [Fact]
    public void NestedSaveRestorePreservesFilterAndSelectedPaints()
    {
        using var stream = new MemoryStream();
        using (IContext context = new SvgDrawingContext(new Size(100, 100), stream, keepOpen: true))
        {
            context.SetFilter(new Brightness(0.5f));
            context.SetFill(Colors.White);
            context.SetStroke(Colors.White);
            context.Save();
            context.SetFilter(new Brightness(0.25f));
            context.SetFill(Colors.White);
            context.FillRect(Bounds);
            context.Save();
            context.SetFilter();
            context.SetFill(Colors.White);
            context.FillRect(Bounds);
            context.Restore();
            context.FillRect(Bounds); // Restored quarter-bright paint.
            context.SetFill(Colors.White);
            context.FillRect(Bounds); // Restored quarter-bright filter.
            context.Restore();
            context.FillRect(Bounds); // Restored half-bright paint.
            context.StrokeRect(Bounds);
            context.SetFill(Colors.White);
            context.FillRect(Bounds); // Restored half-bright filter.
            context.SetFilter();
            context.FillRect(Bounds); // Changing the filter does not recolor an existing paint.
            context.SetFill(Colors.White);
            context.FillRect(Bounds);
        }
        stream.Position = 0;
        var document = XDocument.Load(stream);
        Assert.Equal(new[] { "#3F3F3F", "#FFFFFF", "#3F3F3F", "#3F3F3F", "#7F7F7F", "#7F7F7F", "#7F7F7F", "#FFFFFF" },
            document.Descendants(Ns + "rect").Select(r => (string?)r.Attribute("fill")).Where(f => f != "none"));
        Assert.Equal("#7F7F7F", (string?)Assert.Single(document.Descendants(Ns + "rect"), r => r.Attribute("stroke") is not null).Attribute("stroke"));
    }

    [Fact]
    public void BrightnessAffectsTextAndBothGradientPaintsWithoutChangingAlpha()
    {
        using var stream = new MemoryStream();
        using (IContext context = new SvgDrawingContext(new Size(100, 100), stream, keepOpen: true))
        {
            GradientStop[] stops = [new(0, Colors.White), new(1, new Color(1f, 1f, 1f, 0f))];
            var linear = new LinearGradient((0, 0), (100, 0), stops);
            var radial = new RadialGradient((50, 50), 50, stops);
            context.SetFilter(new Brightness(0.5f));
            context.SetFill(linear);
            context.FillRect(Bounds);
            context.SetStroke(linear);
            context.StrokeRect(Bounds);
            context.SetFill(radial);
            context.FillRect(Bounds);
            context.SetStroke(radial);
            context.StrokeRect(Bounds);
            context.SetFill(new Color(1f, 1f, 1f, 0.5f));
            context.FillText("Brightness", (0, 10));
        }
        stream.Position = 0;
        var document = XDocument.Load(stream);
        var stopsOut = document.Descendants(Ns + "stop").ToArray();
        Assert.Equal(8, stopsOut.Length);
        Assert.All(stopsOut, stop => Assert.Equal("#7F7F7F", (string?)stop.Attribute("stop-color")));
        Assert.All(stopsOut.Where(s => (string?)s.Attribute("offset") == "100%"),
            stop => Assert.Equal("0", (string?)stop.Attribute("stop-opacity")));
        var text = Assert.Single(document.Descendants(Ns + "text"));
        Assert.Equal("#7F7F7F", (string?)text.Attribute("fill"));
        Assert.Equal("0.5", (string?)text.Attribute("fill-opacity"));
    }
}
