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
    public void ImagesMultiplyGlobalAlphaAndRestoreItForLaterDrawing()
    {
        using var bitmap = new Bitmap();
        using IContext context = new IOSDrawingContext();
        var ios = (IOSDrawingContext)context;
        using var factory = new IOSImageFactory();
        var image = (IOSImage)factory.CreateImage();
        image.LoadPixels(1, 1, [255, 255, 255, 255]);
        using var resource = image.Resource!;
        ios.Bind(bitmap.Handle);
        context.GlobalAlpha = (NFloat)0.5;
        context.DrawImage(image, (0, 0, 2, 2), (NFloat)0.5);
        context.Save();
        context.GlobalAlpha = (NFloat)0.75;
        context.DrawImage(image, (2, 0, 2, 2), (NFloat)0.5);
        context.Restore();
        context.DrawImage(image, (4, 0, 2, 2), (NFloat)0.5);
        context.DrawImage(image, (6, 0, 2, 2));
        context.DrawImage(image, (0, 0, 1, 1), (8, 0, 2, 2), (NFloat)0.5);
        context.SetFill(Colors.White);
        context.FillRect((10, 0, 2, 2));
        ios.Unbind();
        Assert.InRange(bitmap.AlphaAt(0), 63, 64);
        Assert.InRange(bitmap.AlphaAt(2), 95, 96);
        Assert.InRange(bitmap.AlphaAt(4), 63, 64);
        Assert.InRange(bitmap.AlphaAt(6), 127, 128);
        Assert.InRange(bitmap.AlphaAt(8), 63, 64);
        Assert.InRange(bitmap.AlphaAt(10), 127, 128);
        ios.Bind(bitmap.Handle);
        context.DrawImage(image, (12, 0, 2, 2));
        ios.Unbind();
        Assert.Equal(255, bitmap.AlphaAt(12));
    }

    [CoreGraphicsFact]
    public void EllipticalArcsKeepTheirEndpointsAndDirectionThroughBothBuffers()
    {
        foreach (var winding in new[] { Winding.ClockWise, Winding.CounterClockWise })
        foreach (bool retained in new[] { false, true })
        foreach (bool mirror in new[] { false, true })
        {
            void Draw(IContext ctx, bool ellipse)
            {
                ctx.Save();
                ctx.Translate((32, 32));
                ctx.Rotate((NFloat).3);
                ctx.Scale((mirror ? -1 : 1, (NFloat).75));
                ctx.SetStroke(Colors.White); ctx.LineWidth = 2;
                ctx.BeginPath();
                var start = -NFloat.Pi / 4; var end = NFloat.Pi / 4;
                // Connected large/small arcs: reversing their endpoints adds a chord.
                ctx.MoveTo((14, -24)); ctx.LineTo((20 * NFloat.Cos(start), 20 * NFloat.Sin(start)));
                if (ellipse) ctx.Ellipse((0, 0), 20, 20, 0, start, end, winding);
                else ctx.Arc((0, 0), 20, start, end, winding);
                ctx.LineTo((14, 24)); ctx.Stroke();
                ctx.Restore();
            }
            var expected = RenderNative(ctx => Draw(ctx, false));
            var actual = RenderNative(ctx => Draw(ctx, true), retained);
            AssertPixelsClose(expected, actual);
        }
    }

    [CoreGraphicsFact]
    public void SvgWashroomCStaysOpenAfterReplay()
    {
        const string svg = """
            <svg xmlns="http://www.w3.org/2000/svg"><path fill="none" stroke="#fff" stroke-width="2"
              stroke-linecap="round" stroke-linejoin="round" d="M3 10l2.5 12L9 14l3.5 8L15 10M29 11a7 7 0 1 0 0 10"/></svg>
            """;
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var drawing = FloorplanDrawing.Load(input);
        void Place(IContext ctx) { ctx.Translate((8, 8)); ctx.Scale(((NFloat)1.5, (NFloat)1.5)); }
        var expected = RenderNative(ctx =>
        {
            Place(ctx); ctx.SetStroke(Colors.White); ctx.LineWidth = 2; ctx.LineCap = LineCap.Round; ctx.LineJoin = LineJoin.Round;
            ctx.BeginPath(); ctx.MoveTo((3, 10)); ctx.LineTo(((NFloat)5.5, 22)); ctx.LineTo((9, 14));
            ctx.LineTo(((NFloat)12.5, 22)); ctx.LineTo((15, 10)); ctx.MoveTo((29, 11));
            var dx = NFloat.Sqrt(24);
            ctx.Arc((29 - dx, 16), 7, NFloat.Atan2(-5, dx), NFloat.Atan2(5, dx), Winding.CounterClockWise);
            ctx.Stroke();
        });
        foreach (bool retained in new[] { false, true })
        {
            var actual = RenderNative(ctx => { Place(ctx); drawing.Render(ctx, 0, (0, 120, 1000, 760)); }, retained);
            AssertPixelsClose(expected, actual);
            Assert.Equal(0, actual[(32 * 64 + 51) * 4 + 3]); // Opening between the two C endpoints.
        }
    }

    [CoreGraphicsFact]
    public void SvgBenchCornersMatchANativeRoundedRectangleAfterReplay()
    {
        // The actual wooden bench outline, including its explicit quarter-circle arcs.
        const string svg = """
            <svg xmlns="http://www.w3.org/2000/svg"><path fill="#c69a63" stroke="#705034" stroke-width="1.4"
              d="M-46-9.6H46A3 3 0 0 1 49-6.6V6.6A3 3 0 0 1 46 9.6H-46A3 3 0 0 1-49 6.6V-6.6A3 3 0 0 1-46-9.6Z"/></svg>
            """;
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var drawing = FloorplanDrawing.Load(input);
        void Place(IContext ctx) { ctx.Translate((32, 32)); ctx.Scale(((NFloat).55, (NFloat)1.5)); }
        var expected = RenderNative(ctx =>
        {
            Place(ctx); ctx.SetFill(new Color(0xc69a63ff)); ctx.SetStroke(new Color(0x705034ff)); ctx.LineWidth = (NFloat)1.4;
            ctx.BeginPath(); ctx.RoundRect((-49, (NFloat)(-9.6), 98, (NFloat)19.2), 3); ctx.Fill(); ctx.Stroke();
        });
        foreach (bool retained in new[] { false, true })
            AssertPixelsClose(expected, RenderNative(ctx => { Place(ctx); drawing.Render(ctx, 3, (0, 120, 1000, 760)); }, retained));
    }

    private static byte[] RenderNative(Action<IContext> draw, bool retained = false)
    {
        using var bitmap = new Bitmap(64);
        using IContext context = new IOSDrawingContext();
        var ios = (IOSDrawingContext)context;
        ios.Bind(bitmap.Handle);
        if (retained)
        {
            using var measure = new SvgDrawingContext((64, 64), Stream.Null);
            using var scene = new RetainedCanvasContext(measure);
            scene.BeginFrame((0, 0, 64, 64), 1); draw(scene); scene.EndFrame();
            scene.Root.Commands.Replay(context);
        }
        else draw(context);
        ios.Unbind();
        return bitmap.Pixels();
    }

    private static void AssertPixelsClose(byte[] expected, byte[] actual)
    {
        // Native rounded-rectangle and unit-circle transforms can differ slightly in antialiasing.
        long difference = 0;
        for (int i = 0; i < expected.Length; i++) difference += Math.Abs(expected[i] - actual[i]);
        Assert.True(difference < 1500, $"Native pixel difference: {difference}");
    }

    [CoreGraphicsFact]
    public void DashReplayRestoresNativePatternAndPhase()
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
        for (int i = 0; i < 100; i++) Replay();

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
