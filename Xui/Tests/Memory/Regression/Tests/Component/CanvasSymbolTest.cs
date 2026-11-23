using System.Text;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Symbols;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public partial class CanvasSymbolTest
{
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
}
