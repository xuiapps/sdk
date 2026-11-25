using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Math2D;
using Xui.Apps.TestApp.Pages.Floorplan;
using System.Text;
using Xui.Runtime.IOS.Actual;
using Xui.Runtime.Software.Actual;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Tests.Component.IOS;

// Exercises the iOS drawing backend against real CoreGraphics on a macOS test host.
// No UIKit window or simulator is needed; portable state tests run on other hosts.
public sealed partial class IOSDrawingContextTest
{
    [CoreGraphicsFact]
    public void DashReplayRestoresNativePatternAndPhaseWithoutAllocating()
    {
        using var bitmap = new Bitmap();
        using IContext context = new IOSDrawingContext();
        var ios = (IOSDrawingContext)context;
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((16, 16), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 16, 16), 1);
        scene.SetLineDash([2, 4]);
        scene.LineDashOffset = 1;
        scene.Save();
        scene.SetLineDash([1, 2, 3]);
        scene.LineDashOffset = 2;
        scene.Save();
        scene.SetLineDash([]);
        scene.Restore();
        scene.Restore();
        // Changing only the phase after Restore must use the restored pattern.
        scene.LineDashOffset = 5;
        scene.EndFrame();

        void Replay()
        {
            ios.Bind(bitmap.Handle);
            scene.Root.Commands.Replay(context);
            ios.Unbind();
        }
        for (int i = 0; i < 20; i++) Replay();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) Replay();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);

        Stroke(bitmap.Handle);
        using var expected = new Bitmap();
        Span<NFloat> dashes = [2, 4];
        CGContextRef.CGContextSetLineDash(expected.Handle, 5, ref MemoryMarshal.GetReference(dashes), 2);
        Stroke(expected.Handle);
        Assert.Equal(expected.Pixels(), bitmap.Pixels());

        // A new native drawing pass starts solid, even when only its phase changes.
        using var next = new Bitmap();
        ios.Bind(next.Handle);
        context.LineDashOffset = 3;
        Stroke(next.Handle);
        using var solid = new Bitmap();
        Stroke(solid.Handle);
        Assert.Equal(solid.Pixels(), next.Pixels());
        Assert.False(solid.Pixels().AsSpan().SequenceEqual(expected.Pixels()));
        ios.Unbind();
    }
}
