using System.Xml.Linq;
using System.Globalization;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class SvgGradientOpacityTest
{
    [Theory]
    [InlineData("bg-BG")]
    [InlineData("en-US")]
    public void FractionalGradientOffsetsUseInvariantFormatting(string cultureName)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            using var stream = new MemoryStream();
            using (IContext context = new SvgDrawingContext(new Size(100, 100), stream, keepOpen: true))
            {
                GradientStop[] stops = [new(0, Colors.Black), new((nfloat)5 / 6, Colors.Black), new(1, Colors.White)];
                var linear = new LinearGradient((0, 0), (100, 0), stops);
                var radial = new RadialGradient((50, 50), 50, stops);
                context.SetFill(linear);
                context.SetStroke(linear);
                context.SetFill(radial);
                context.SetStroke(radial);
            }
            stream.Position = 0;
            XNamespace ns = "http://www.w3.org/2000/svg";
            var offsets = XDocument.Load(stream).Descendants(ns + "stop")
                .Select(stop => (string?)stop.Attribute("offset")).ToArray();
            Assert.Equal(4, offsets.Count(offset => offset == "83.3%"));
            Assert.DoesNotContain(offsets, offset => offset!.Contains(','));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void TransparentStopsRemainTransparentForFillAndStrokeGradients()
    {
        using var stream = new MemoryStream();
        using (IContext context = new SvgDrawingContext(new Size(100, 100), stream, keepOpen: true))
        {
            GradientStop[] stops = [new(0, Colors.Black), new(1, new Color(0x00000000u))];
            var linear = new LinearGradient((0, 0), (100, 0), stops);
            var radial = new RadialGradient((50, 50), 50, (50, 50), 0, stops);
            context.SetFill(linear);
            context.FillRect(new Rect(0, 0, 100, 100));
            context.SetFill(radial);
            context.FillRect(new Rect(0, 0, 100, 100));
            context.SetStroke(linear);
            context.StrokeRect(new Rect(0, 0, 100, 100));
            context.SetStroke(radial);
            context.StrokeRect(new Rect(0, 0, 100, 100));
        }
        stream.Position = 0;
        var document = XDocument.Load(stream);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var transparentStops = document.Descendants(ns + "stop")
            .Where(stop => (string?)stop.Attribute("offset") == "100%").ToArray();
        Assert.Equal(4, transparentStops.Length);
        Assert.All(transparentStops, stop => Assert.Equal("0", (string?)stop.Attribute("stop-opacity")));
    }
}
