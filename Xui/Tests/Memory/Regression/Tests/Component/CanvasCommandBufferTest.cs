using Xui.Core.Canvas.Composition.Commands.Records;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public partial class CanvasCommandBufferTest
{
    [Fact]
    public void WarmRecordingDoesNotAllocatePerCommand()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var image = new SoftwareImagePipeline().CreateImage();
        void Record()
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            for (var i = 0; i < 128; i++)
            {
                scene.Save();
                scene.SetFill(Colors.Red);
                scene.SetFont(new Xui.Core.Canvas.Font(16, "Inter"));
                scene.FillText("Packed text", (10, 20));
                scene.FillText("Copied text".AsSpan(), (10, 40));
                scene.DrawImage(image, (1, 2, 3, 4));
                scene.FillRect((i, 1, 30, 40));
                scene.Restore();
            }
            scene.EndFrame();
            scene.AcceptFrame();
        }
        for (var i = 0; i < 10; i++) Record();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10; i++) Record();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
