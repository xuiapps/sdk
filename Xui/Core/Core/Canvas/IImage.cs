using Xui.Core.Math2D;

namespace Xui.Core.Canvas;

/// <summary>
/// Represents a decoded, GPU-resident image that can be drawn via <see cref="IImageDrawingContext"/>.
/// Acquire instances through <see cref="IImageFactory.Load"/>.
/// </summary>
public interface IImage
{
    /// <summary>Intrinsic size of the image in pixels.</summary>
    Size Size { get; }
}
