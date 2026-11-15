using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xui.Apps.TestApp;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;
using Xui.Tests.Integration.TestApp;
namespace Xui.Tests.Integration.Composition;

public class CompositionRenderingTest
{
    [Theory]
    [InlineData("AnimatedHeart")]
    [InlineData("Basics")]
    [InlineData("GridLayout")]
    [InlineData("FlexboxLayout")]
    [InlineData("Gestures")]
    [InlineData("TextBox")]
    [InlineData("DeviceInfo")]
    [InlineData("Haptics")]
    [InlineData("VibrationLive")]
    [InlineData("VibrationRecord")]
    public void MobileExamplesKeepHeaderAboveRetainedContent(string id)
    {
        using var app = IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((600, 400),
            TestRuntimeVariant.IPhoneEmulator, SourcePath(), nameof(MobileExamplesKeepHeaderAboveRetainedContent), Xui.Core.Fonts.Inter.Source);
        app.Render();
        Assert.IsAssignableFrom<Button>(app.Window.RootView.FindViewById(id)).Clicked!();
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        app.Render();
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((600, 1000), output, Xui.Core.Fonts.Inter.URIs, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        for (var i = 0; i < 2; i++)
        {
            app.AnimationFrame(TimeSpan.FromSeconds(1 + i), TimeSpan.FromSeconds(2 + i));
            scene.BeginFrame(app.Window.DisplayArea, 3);
            app.Window.RootView.Render(scene);
            scene.EndFrame();
            var page = Nodes(scene.Root).Single(n => Enumerable.Range(0, n.Count).Any(i => n[i].Options.Backdrop == BackdropMaterial.Blur));
            Assert.Equal(BackdropMaterial.Blur, page[page.Count - 1].Options.Backdrop);
            scene.AcceptFrame();
        }
    }

    [Fact]
    public void CarouselCardsReuseDrawingsWhileMovingAndAfterCulling()
    {
        using var app = IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((600, 400),
            TestRuntimeVariant.IPhoneEmulator, SourcePath(), nameof(CarouselCardsReuseDrawingsWhileMovingAndAfterCulling), Xui.Core.Fonts.Inter.Source);
        app.Render();
        Assert.IsAssignableFrom<Button>(app.Window.RootView.FindViewById("CanvasTests")).Clicked!();
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        app.Render();
        var carousel = app.Window.RootView.FindViewById("CanvasDrawingCarousel")!;
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((600, 1000), output, Xui.Core.Fonts.Inter.URIs, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        void Record()
        {
            scene.BeginFrame(app.Window.DisplayArea, 3);
            app.Window.RootView.Render(scene);
            scene.EndFrame();
        }
        Record(); scene.AcceptFrame();
        var first = Nodes(scene.Root).Single(n => ReferenceEquals(n.Options.Key, carousel[0]));
        var card = Assert.Single(Enumerable.Range(0, first.Count).Select(i => first[i]), n => n.Options.Key is Card);
        Assert.NotNull(card.Options.Shadow);
        Assert.NotNull(card.Options.Border);
        var originalPlacement = first.Placement;
        var y = carousel.Frame.Center.Y;
        app.PointerDown((310, y)); app.PointerMove((240, y));
        Record();
        Assert.Same(first, Nodes(scene.Root).Single(n => ReferenceEquals(n.Options.Key, carousel[0])));
        Assert.NotEqual(originalPlacement.Tx, first.Placement.Tx);
        Assert.False(first.Updating);
        Assert.False(card.Updating);
        scene.AcceptFrame();
        app.PointerMove((100, y)); app.PointerUp((100, y));
        app.AnimationFrame(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1.1));
        app.AnimationFrame(TimeSpan.FromSeconds(1.1), TimeSpan.FromSeconds(2));
        Record(); scene.AcceptFrame();
        app.PointerDown((310, y)); app.PointerMove((100, y)); app.PointerUp((100, y));
        app.AnimationFrame(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2.1));
        app.AnimationFrame(TimeSpan.FromSeconds(2.1), TimeSpan.FromSeconds(3));
        Record(); scene.AcceptFrame();
        Assert.DoesNotContain(first, Nodes(scene.Root));
        Assert.Contains(Nodes(scene.Root), n => n is GraphicsNode graphics && graphics.HiddenChildren.Contains(first));
        app.PointerDown((100, y)); app.PointerMove((310, y)); app.PointerUp((310, y));
        app.AnimationFrame(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3.1));
        app.AnimationFrame(TimeSpan.FromSeconds(3.1), TimeSpan.FromSeconds(4));
        Record();
        Assert.Contains(first, Nodes(scene.Root));
        Assert.False(first.Updating);
    }

    [Fact]
    public void PagesScrollSurfacesAndGlassToolbar()
    {
        var path=SourcePath();
        using var app=IntegrationRuntimeVariants.CreateApp<Application,MainWindow>((600,400),TestRuntimeVariant.IPhoneEmulator,path,nameof(PagesScrollSurfacesAndGlassToolbar),Xui.Core.Fonts.Inter.Source);
        app.SvgFontResolver = SvgDrawingContext.SvgFontResolver.Embedded;
        app.MarkdownParagraph("# Retained page composition\n\nPages, cards, and scrolling viewports are separate keyed surfaces. Card borders and shadows follow rounded outlines, and culled cards can reuse their retained drawings. The toolbar uses native Liquid Glass with a translucent selection. Headers blur the content scrolling beneath them, including the top safe area. SVG approximates the materials while keeping labels crisp.");
        app.Render();
        using var measureOutput=new MemoryStream();
        using var measure=new SvgDrawingContext((600,1000),measureOutput,Xui.Core.Fonts.Inter.URIs,keepOpen:true);
        using var scene=new RetainedCanvasContext(measure);
        void Record()
        {
            scene.BeginFrame(app.Window.DisplayArea,3);
            app.Window.RootView.Render(scene);
            scene.EndFrame(); scene.AcceptFrame();
            Assert.True(scene.Root.Count > 0);
        }
        Record(); Record();
        Assert.Contains("data:font/ttf;base64,", app.Snapshot("Home"));
        Assert.IsAssignableFrom<Button>(app.Window.RootView.FindViewById("DesignSystem")).Clicked!();
        app.AnimationFrame(TimeSpan.Zero,TimeSpan.Zero); app.Render();
        app.AnimationFrame(TimeSpan.Zero,TimeSpan.FromMilliseconds(16)); app.Render();
        Record();
        // The page is mostly offscreen, but its cached header must already contain its title.
        var header = Nodes(scene.Root).Single(n => n.Options.Key is Xui.Apps.TestApp.Pages.MobileExampleHeader);
        using (var headerOutput = new MemoryStream())
        {
            using (var svg = new SvgDrawingContext((600, 1000), headerOutput, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
            { var graphics = Assert.IsType<GraphicsNode>(header); graphics.Inherited.Replay(svg); graphics.Commands.Replay(svg); }
            headerOutput.Position = 0;
            Assert.Contains(XDocument.Load(headerOutput).Descendants(XName.Get("text", "http://www.w3.org/2000/svg")),
                text => text.Value == "Design System");
        }
        app.AnimationFrame(TimeSpan.FromMilliseconds(16),TimeSpan.FromMilliseconds(100)); app.Render();
        Record();
        var transition=XDocument.Parse(app.Snapshot("PageTransition"));
        Assert.Contains("data:font/ttf;base64,", transition.ToString());
        XNamespace ns="http://www.w3.org/2000/svg";
        Assert.NotEmpty(transition.Descendants(ns+"feGaussianBlur"));
        app.AnimationFrame(TimeSpan.FromMilliseconds(100),TimeSpan.FromMilliseconds(1000)); app.Render();
        Record();
        var document=XDocument.Parse(app.Snapshot("GlassToolbar"));
        Assert.Contains("data:font/ttf;base64,", document.ToString());
        Assert.Single(document.Descendants(ns+"rect"),e=>(string?)e.Attribute("data-xui-material")=="LiquidGlass");
        var glass=Nodes(scene.Root).Single(n=>n.Options.Backdrop==BackdropMaterial.LiquidGlass);
        Assert.NotNull(glass.Options.Shadow);
        Assert.True(Assert.IsType<GraphicsNode>(glass).HasDrawing);
        Assert.Contains(Nodes(scene.Root),n=>n.Options.ClipToBounds);
        var blur = Nodes(scene.Root).Single(n => n.Options.Backdrop == BackdropMaterial.Blur);
        Assert.Equal(app.Window.DisplayArea.Top, blur.RasterBounds.Top);
        Assert.Contains(Nodes(scene.Root), n => n.Options.Key is Card && n.Options.Border is not null && n.Options.Shadow is not null);
        app.PointerDown((360, 700)); app.PointerMove((360, 300)); app.PointerUp((360, 300));
        app.Render(); Record();
        Assert.Contains("data:font/ttf;base64,", app.Snapshot("BlurredHeader"));
    }
    private static string SourcePath([CallerFilePath] string path="") => path;
    private static IEnumerable<Node> Nodes(Node node)
    {
        yield return node;
        for(var i=0;i<node.Count;i++) foreach(var descendant in Nodes(node[i])) yield return descendant;
    }
}
