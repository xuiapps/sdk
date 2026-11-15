using System;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Decoded CoreGraphics image owned by the iOS window image catalog.</summary>
internal sealed class IOSImageResource : IDisposable
{
    internal CoreGraphics.CGImage? CGImage;

    internal uint Width => CGImage?.Width ?? 0;
    internal uint Height => CGImage?.Height ?? 0;

    internal IOSImageResource(CoreGraphics.CGImage image) => CGImage = image;

    public void Dispose()
    {
        CGImage?.Dispose();
        CGImage = null;
    }
}
