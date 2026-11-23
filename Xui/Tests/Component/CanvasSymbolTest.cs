using System.Text;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Symbols;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class CanvasSymbolTest
{
    private static void Define(IContext context, object key, Color color, bool update = false)
    {
        using var symbol = context.Symbol(new SymbolOptions { Key = key, Bounds = (0, 0, 20, 20), HasUpdates = update });
        if (symbol.NeedsDrawing)
        {
            symbol.Context.SetFill(color);
            symbol.Context.FillRect((2, 2, 16, 16));
        }
    }

    [Fact]
    public void DefinitionDoesNotDrawAndSvgUsesOneDefinitionForManyPlacements()
    {
        using var stream = new MemoryStream();
        using (IContext context = new SvgDrawingContext((200, 200), stream, keepOpen: true))
        {
            object key = new();
            Define(context, key, Colors.Red);
            using (var cached = context.Symbol(new SymbolOptions { Key = key, Bounds = (0, 0, 20, 20) }))
                Assert.False(cached.NeedsDrawing);
            for (int i = 0; i < 10; i++)
            {
                context.Save(); context.Translate((i * 20, 0)); context.UseSymbol(key); context.Restore();
            }
        }
        var svg = XDocument.Parse(Encoding.UTF8.GetString(stream.ToArray()));
        XNamespace ns = "http://www.w3.org/2000/svg";
        var definition = Assert.Single(svg.Descendants(ns + "symbol"));
        Assert.Equal("symbol1", (string?)definition.Attribute("id"));
        Assert.Equal("0 0 20 20", (string?)definition.Attribute("viewBox"));
        Assert.Equal(10, svg.Descendants(ns + "use").Count());
        Assert.Single(svg.Descendants(ns + "rect"));
        Assert.NotNull(svg.Descendants(ns + "rect").Single().Ancestors(ns + "defs").SingleOrDefault());
    }

    [Theory]
    [InlineData(-12.5, 4.25)]
    [InlineData(10.5, -8.25)]
    public void SvgSymbolViewportPreservesNonzeroLocalCoordinates(double x, double y)
    {
        using var stream = new MemoryStream();
        var bounds = new Xui.Core.Math2D.Rect((nfloat)x, (nfloat)y, 20, 30);
        using (IContext context = new SvgDrawingContext((200, 200), stream, keepOpen:true))
        {
            object key = new();
            using (var symbol = context.Symbol(new SymbolOptions { Key=key, Bounds=bounds }))
                symbol.Context.FillRect(bounds);
            context.Translate((100, 50)); context.Rotate((nfloat).4);
            context.UseSymbol(key);
        }
        var svg = XDocument.Parse(Encoding.UTF8.GetString(stream.ToArray()));
        XNamespace ns = "http://www.w3.org/2000/svg";
        var definition = Assert.Single(svg.Descendants(ns+"symbol"));
        var use = Assert.Single(svg.Descendants(ns+"use"));
        Assert.Equal(FormattableString.Invariant($"{x} {y} 20 30"), (string?)definition.Attribute("viewBox"));
        Assert.Equal(x, (double)use.Attribute("x")!);
        Assert.Equal(y, (double)use.Attribute("y")!);
        Assert.Equal(20, (double)use.Attribute("width")!);
        Assert.Equal(30, (double)use.Attribute("height")!);
        Assert.Equal("none", (string?)definition.Attribute("preserveAspectRatio"));
        Assert.Contains(use.Ancestors(ns+"g"), g => g.Attribute("transform") is not null);
    }

    [Fact]
    public void UpdatedKeyCannotMutateEarlierRecordedPlacements()
    {
        using var measure = new SvgDrawingContext((100, 100), Stream.Null);
        using var scene = new RetainedCanvasContext(measure);
        IContext context = scene;
        object key = new();
        scene.BeginFrame((0, 0, 100, 100), 1);
        Define(context, key, Colors.Red);
        context.UseSymbol(key);
        Define(context, key, Colors.Blue, update: true);
        context.UseSymbol(key);
        context.ReleaseSymbol(key);
        Assert.Throws<InvalidOperationException>(() => context.UseSymbol(key));
        scene.EndFrame();
        using var stream = new MemoryStream();
        using (IContext svg = new SvgDrawingContext((100, 100), stream, keepOpen: true))
            scene.Root.Commands.Replay(svg);
        var text = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("#FF0000", text); Assert.Contains("#0000FF", text);
        Assert.Contains("#symbol1", text); Assert.Contains("#symbol2", text);
    }

    [Fact]
    public void CachedDefinitionAndPlacementsAllocateNothingAfterWarmup()
    {
        using var measure = new SvgDrawingContext((100, 100), Stream.Null);
        using var scene = new RetainedCanvasContext(measure);
        IContext context = scene;
        object key = new();
        void Frame()
        {
            scene.BeginFrame((0, 0, 100, 100), 1);
            Define(context, key, Colors.Red);
            for (int i=0; i<100; i++) context.UseSymbol(key);
            scene.EndFrame(); scene.AcceptFrame();
        }
        for (int i=0; i<20; i++) Frame();
        long before=GC.GetAllocatedBytesForCurrentThread();
        for (int i=0; i<100; i++) Frame();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread()-before);
        Assert.Equal(103, scene.Root.Commands.Count); // initial font/alignment/baseline + placements
    }

    [Fact]
    public void FailedDefinitionPreservesOldContentAndNewContextRequestsDrawing()
    {
        using IContext context = new SvgDrawingContext((100, 100), Stream.Null);
        object key = new();
        Define(context, key, Colors.Red);
        using (var failed = context.Symbol(new SymbolOptions { Key=key, Bounds=(0,0,20,20), HasUpdates=true }))
        { failed.Context.SetFill(Colors.Blue); failed.Cancel(); }
        context.UseSymbol(key);
        using IContext other = new SvgDrawingContext((100, 100), Stream.Null);
        using var fresh = other.Symbol(new SymbolOptions { Key=key, Bounds=(0,0,20,20) });
        Assert.True(fresh.NeedsDrawing);
        fresh.Cancel();
    }

    [Fact]
    public void TextAndGradientDefinitionsKeepFontSubsettingAndRestoreTextState()
    {
        using var stream=new MemoryStream();
        using (IContext context=new SvgDrawingContext((200,100), stream, Xui.Core.Fonts.Inter.Source,
            resolver:SvgDrawingContext.SvgFontResolver.Embedded, keepOpen:true))
        {
            context.SetFont(new Xui.Core.Canvas.Font(25,"Inter")); context.TextAlign=TextAlign.Right;
            object key=new();
            using (var symbol=context.Symbol(new SymbolOptions { Key=key, Bounds=(0,0,100,60) }))
            {
                symbol.Context.SetFont(new Xui.Core.Canvas.Font(14,"Inter"));
                symbol.Context.SetFill(new LinearGradient((0,0),(100,0),[new(0,Colors.Red),new(1,Colors.Blue)]));
                symbol.Context.FillText("Symbol",(2,30));
            }
            context.UseSymbol(key);
            context.FillText("Outside",(180,80));
        }
        var document=XDocument.Parse(Encoding.UTF8.GetString(stream.ToArray()));
        XNamespace ns="http://www.w3.org/2000/svg";
        var outside=document.Descendants(ns+"text").Single(e=>e.Value=="Outside");
        Assert.Equal("25",outside.Attribute("font-size")?.Value);
        Assert.Equal("end",outside.Attribute("text-anchor")?.Value);
        Assert.Single(document.Descendants(ns+"linearGradient"));
        Assert.Contains("data:font/ttf;base64,", document.ToString());
    }
}
