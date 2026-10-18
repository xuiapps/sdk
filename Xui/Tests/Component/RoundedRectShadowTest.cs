using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.DevKit.UI.Drawing;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class RoundedRectShadowTest
{
    private static readonly XNamespace Ns = "http://www.w3.org/2000/svg";

    [Fact]
    public void UniformShadowUsesEightEdgePiecesAndTwoGradientsAndRestoresPaint()
    {
        var document = Render(context =>
        {
            context.SetFill(Colors.Red);
            RoundedRectShadow.Draw(context, new Rect(20, 20, 100, 60), 12, 6,
                new Color(0f, 0f, 0f, 0.12f));
            context.FillRect(new Rect(150, 20, 2, 2));
        });
        Assert.Single(document.Descendants(Ns + "linearGradient"));
        Assert.Single(document.Descendants(Ns + "radialGradient"));
        Assert.Equal(4, document.Descendants(Ns + "path").Count());
        var strips = document.Descendants(Ns + "rect")
            .Where(r => ((string?)r.Attribute("fill"))?.StartsWith("url(") == true).ToArray();
        Assert.Equal(4, strips.Length);
        Assert.All(strips, r => Assert.Equal("6", (string?)r.Attribute("width")));
        Assert.Equal(new[] { "36", "36", "76", "76" }, strips.Select(r => (string)r.Attribute("height")!).Order());
        Assert.Empty(document.Descendants(Ns + "filter"));
        Assert.Empty(document.Descendants(Ns + "image"));
        Assert.Empty(document.Descendants(Ns + "clipPath"));
        var marker = Assert.Single(document.Descendants(Ns + "rect"), r => (string?)r.Attribute("x") == "150");
        Assert.Equal("#FF0000", (string?)marker.Attribute("fill"));
        Assert.DoesNotContain(marker.Ancestors(), p => p.Attribute("transform") is not null);
        Assert.All(document.Descendants(Ns + "stop").Where(s => (string?)s.Attribute("offset") == "100%"),
            s => Assert.Equal("0", (string?)s.Attribute("stop-opacity")));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(8, 16, 24, 0)]
    [InlineData(1000, 1000, 1000, 1000)]
    public void HandlesSquareAsymmetricAndOversizedCorners(double tl, double tr, double br, double bl)
    {
        var document = Render(context => RoundedRectShadow.Draw(context, new Rect(20, 20, 100, 60),
            new CornerRadius((nfloat)tl, (nfloat)tr, (nfloat)br, (nfloat)bl), 6, Colors.Black));
        Assert.Equal(4, document.Descendants(Ns + "path").Count());
        Assert.DoesNotContain("NaN", document.ToString());
        Assert.DoesNotContain("Infinity", document.ToString());
        Assert.All(document.Descendants(Ns + "rect"), r =>
            Assert.True(double.Parse((string)r.Attribute("height")!, System.Globalization.CultureInfo.InvariantCulture) > 0));
    }

    [Fact]
    public void DisabledAndEmptyShadowsEmitNoPaint()
    {
        var document = Render(context =>
        {
            RoundedRectShadow.Draw(context, new Rect(20, 20, 100, 60), 12, 0, Colors.Black);
            RoundedRectShadow.Draw(context, new Rect(20, 20, 100, 60), 12, 6, new Color(0x00000000u));
            RoundedRectShadow.Draw(context, new Rect(20, 20, 0, 60), 12, 6, Colors.Black);
        });
        Assert.Empty(document.Descendants(Ns + "path"));
        Assert.Empty(document.Descendants(Ns + "rect"));
        Assert.Empty(document.Descendants(Ns + "linearGradient"));
        Assert.Empty(document.Descendants(Ns + "radialGradient"));
    }

    [Fact]
    public void CardCullingIncludesShadowWithoutChangingLayoutFrame()
    {
        var bounds = new Rect(20, 20, 100, 60);
        var card = new PositionedCard(bounds) { ShadowExtent = 6 };
        Assert.Equal(bounds, card.Frame);
        Assert.Equal(new Rect(14, 14, 112, 72), card.CullingFrame);
        card.ShadowColor = new Color(0x00000000u);
        Assert.Equal(bounds, card.CullingFrame);
    }

    private sealed class PositionedCard : Card
    {
        public PositionedCard(Rect bounds) => Frame = bounds;
    }

    private static XDocument Render(Action<IContext> draw)
    {
        using var stream = new MemoryStream();
        using (IContext context = new SvgDrawingContext(new Size(200, 120), stream, keepOpen: true))
            draw(context);
        stream.Position = 0;
        return XDocument.Load(stream);
    }
}
