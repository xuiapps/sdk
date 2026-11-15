using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Runtime.IOS.Actual;
using Xui.Runtime.Software.Actual;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Tests.Component.IOS;

// Exercises the iOS drawing backend against real CoreGraphics on a macOS test host.
// No UIKit window or simulator is needed; portable state tests run on other hosts.
public sealed partial class IOSDrawingContextTest
{
    public sealed class CoreGraphicsFactAttribute : FactAttribute
    {
        public CoreGraphicsFactAttribute()
        {
            if (!OperatingSystem.IsMacOS()) Skip = "Requires Apple's CoreGraphics framework.";
        }
    }

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

    private static void Stroke(nint context)
    {
        CGContextRef.CGContextSetRGBStrokeColor(context, 1, 1, 1, 1);
        CGContextRef.CGContextStrokeRect(context, new CGRect(1, 4, 14, 8));
    }

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

    private sealed class Bitmap : IDisposable
    {
        internal nint Handle { get; }
        internal Bitmap()
        {
            using var space = CGColorSpaceRef.CreateDeviceRGB();
            Handle = CGBitmapContextCreate(0, 16, 16, 8, 64, space,
                CGBitmapInfo.ByteOrder32Little | CGBitmapInfo.AlphaPremultipliedFirst);
            Assert.NotEqual(0, Handle);
        }
        internal int AlphaAt(int x)
        {
            // Sample the drawn column independently of Quartz's row orientation.
            int alpha = 0;
            for (int y = 0; y < 16; y++)
                alpha = Math.Max(alpha, Marshal.ReadByte(CGBitmapContextGetData(Handle), y * 64 + x * 4 + 3));
            return alpha;
        }
        internal byte[] Pixels()
        {
            var pixels = new byte[16 * 64];
            Marshal.Copy(CGBitmapContextGetData(Handle), pixels, 0, pixels.Length);
            return pixels;
        }
        public void Dispose() => CGContextRef.CGContextRelease(Handle);
    }

    [LibraryImport(CoreGraphicsLib)]
    private static partial nint CGBitmapContextCreate(nint data, nuint width, nuint height,
        nuint bitsPerComponent, nuint bytesPerRow, nint colorSpace, CGBitmapInfo bitmapInfo);
    [LibraryImport(CoreGraphicsLib)]
    private static partial nint CGBitmapContextGetData(nint context);
}
