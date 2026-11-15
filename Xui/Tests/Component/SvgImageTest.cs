using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Software;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class SvgImageTest
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

    [Fact]
    public void ImagesEmbedOnceWithOriginalPixelsAndSupportCropOpacityAndClipping()
    {
        var image = new SoftwareImagePipeline().CreateImage();
        image.LoadPixels(2, 1, [0, 0, 255, 255, 0, 255, 0, 128]);
        var document = Render(context =>
        {
            context.Save();
            context.Translate((7, 9));
            context.BeginPath(); context.Rect((0, 0, 80, 70)); context.Clip();
            context.GlobalAlpha = (nfloat)0.5;
            context.DrawImage(image, (10, 20, 40, 30));
            context.DrawImage(image, (1, 0, 1, 1), (30, 40, 20, 10), (nfloat)0.4);
            context.Restore();
        });
        var embedded = Assert.Single(document.Descendants(Svg + "image"));
        var dataUri = (string)embedded.Attribute(XLink + "href")!;
        Assert.StartsWith("data:image/png;base64,", dataUri);
        using var png = new MemoryStream(Convert.FromBase64String(dataUri.Split(',')[1]));
        Assert.Equal(new byte[] {255, 0, 0, 255, 0, 255, 0, 128}, PngDecoder.LoadRGBA(png, out var width, out var height));
        Assert.Equal(2, width); Assert.Equal(1, height);
        var draws = document.Descendants(Svg + "svg").Skip(1).ToArray();
        Assert.Equal(2, draws.Length);
        Assert.Equal("1 0 1 1", (string?)draws[1].Attribute("viewBox"));
        Assert.Equal("0.2", (string?)draws[1].Attribute("opacity"));
        Assert.Equal("hidden", (string?)draws[1].Attribute("overflow"));
        Assert.Contains(draws[1].Ancestors(), element => element.Attribute("clip-path") is not null);
        Assert.All(document.Descendants(Svg + "use"), use => Assert.Equal("#image1", (string?)use.Attribute(XLink + "href")));
    }

    [Theory]
    [InlineData(PatternRepeat.Repeat)]
    [InlineData(PatternRepeat.RepeatX)]
    [InlineData(PatternRepeat.RepeatY)]
    [InlineData(PatternRepeat.NoRepeat)]
    public void PatternsRespectRepeatAxesAndSaveRestore(PatternRepeat repeat)
    {
        var image = new SoftwareImagePipeline().CreateImage();
        image.LoadPixels(2, 1, [0, 0, 255, 255, 0, 255, 0, 128]);
        var document = Render(context =>
        {
            context.SetFill(new ImagePattern(image, repeat));
            context.Save();
            context.SetFill(Colors.Blue); context.FillRect((0, 0, 10, 10));
            context.Restore();
            context.Scale((2, 2));
            context.FillRect((0, 0, 30, 30));
            context.SetStroke(new ImagePattern(image, repeat));
            context.StrokeRect((0, 0, 30, 30));
        });
        Assert.Single(document.Descendants(Svg + "image"));
        var patterns = document.Descendants(Svg + "pattern").ToArray();
        Assert.Equal(2, patterns.Length);
        foreach (var pattern in patterns)
        {
            var width = (double)pattern.Attribute("width")!;
            var height = (double)pattern.Attribute("height")!;
            if (repeat is PatternRepeat.Repeat or PatternRepeat.RepeatX) Assert.Equal(2, width);
            else Assert.True(width >= 50);
            if (repeat is PatternRepeat.Repeat or PatternRepeat.RepeatY) Assert.Equal(1, height);
            else Assert.True(height >= 50);
        }
        Assert.Contains(document.Descendants(Svg + "rect"), rect => (string?)rect.Attribute("fill") == "url(#pattern1)");
        Assert.Contains(document.Descendants(Svg + "rect"), rect => (string?)rect.Attribute("stroke") == "url(#pattern2)");
    }

    [Fact]
    public async Task PackagedImagesAreCachedAndEmbeddedWithoutReencoding()
    {
        var directory = Path.Combine(Path.GetTempPath(), "xui-svg-images-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "Assets"));
        try
        {
            var path = Path.Combine(directory, "Assets", "sample.png");
            PngEncoder.SaveRGBA(path, [255, 0, 0, 255], 1, 1);
            var bytes = File.ReadAllBytes(path);
            var pipeline = new SoftwareImagePipeline(directory);
            var first = pipeline.CreateImage(); first.Load("asset://sample.png");
            File.Delete(path); // A second handle uses the cached resource.
            var second = pipeline.CreateImage(); await second.LoadAsync("asset://sample.png");
            Assert.Equal(new Size(1, 1), second.Size);
            var document = Render(context => { context.DrawImage(first, (0, 0, 20, 20)); context.DrawImage(second, (30, 0, 20, 20)); });
            var embedded = Assert.Single(document.Descendants(Svg + "image"));
            Assert.Equal("data:image/png;base64," + Convert.ToBase64String(bytes), (string?)embedded.Attribute(XLink + "href"));
        }
        finally { Directory.Delete(directory, true); }
    }

    private static XDocument Render(Action<IContext> draw)
    {
        using var stream = new MemoryStream();
        using (IContext context = new SvgDrawingContext((100, 100), stream, keepOpen: true)) draw(context);
        stream.Position = 0;
        return XDocument.Load(stream);
    }
}
