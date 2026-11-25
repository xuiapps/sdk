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
    public sealed class CoreGraphicsFactAttribute : FactAttribute
    {
        public CoreGraphicsFactAttribute()
        {
            if (!OperatingSystem.IsMacOS()) Skip = "Requires Apple's CoreGraphics framework.";
        }
    }

    private static void Stroke(nint context)
    {
        CGContextRef.CGContextSetRGBStrokeColor(context, 1, 1, 1, 1);
        CGContextRef.CGContextStrokeRect(context, new CGRect(1, 4, 14, 8));
    }

    private sealed class Bitmap : IDisposable
    {
        internal nint Handle { get; }
        private readonly int size;
        internal Bitmap(int size = 16)
        {
            this.size = size;
            using var space = CGColorSpaceRef.CreateDeviceRGB();
            Handle = CGBitmapContextCreate(0, (nuint)size, (nuint)size, 8, (nuint)(size * 4), space,
                CGBitmapInfo.ByteOrder32Little | CGBitmapInfo.AlphaPremultipliedFirst);
            Assert.NotEqual(0, Handle);
        }
        internal int AlphaAt(int x)
        {
            // Sample the drawn column independently of Quartz's row orientation.
            int alpha = 0;
            for (int y = 0; y < size; y++)
                alpha = Math.Max(alpha, Marshal.ReadByte(CGBitmapContextGetData(Handle), y * size * 4 + x * 4 + 3));
            return alpha;
        }
        internal byte[] Pixels()
        {
            var pixels = new byte[size * size * 4];
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
