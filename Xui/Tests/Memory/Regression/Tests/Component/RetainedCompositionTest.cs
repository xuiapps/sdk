using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
namespace Xui.Tests.Component;

public partial class RetainedCompositionTest
{
    [Fact]
    public void NodeEnumerationUsesPresentationOrderWithoutAllocations()
    {
        using var output = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), output, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        for (var i = 0; i < 3; i++)
            using (scene.Group(new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 100, 100) }))
                scene.FillRect((0, 0, 10, 10));
        scene.EndFrame();
        Node root = scene.Root;
        var visits = 0;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var repeat = 0; repeat < 100; repeat++)
        {
            var index = 0;
            foreach (var child in root)
            {
                if (!ReferenceEquals(root[index++], child)) throw new InvalidOperationException("Unexpected child order.");
                visits++;
            }
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(300, visits);
        Assert.Equal(0, allocated);
        Assert.Equal(0, root[0].Count);
        Assert.False(root[0].GetEnumerator().MoveNext());
    }
}
