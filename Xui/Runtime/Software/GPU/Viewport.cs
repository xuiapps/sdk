using System;

namespace Xui.GPU.Software;

/// <summary>
/// Defines the viewport transformation from normalized device coordinates to screen space.
/// </summary>
/// <remarks>
/// The viewport maps coordinates from NDC space [-1, 1] to screen space [x, x+width] and [y, y+height].
/// The depth range maps from NDC space [0, 1] to [minDepth, maxDepth].
/// </remarks>
public readonly struct Viewport
{
    /// <summary>
    /// Gets the X coordinate of the viewport's upper-left corner in pixels.
    /// </summary>
    public readonly float X;

    /// <summary>
    /// Gets the Y coordinate of the viewport's upper-left corner in pixels.
    /// </summary>
    public readonly float Y;

    /// <summary>
    /// Gets the width of the viewport in pixels.
    /// </summary>
    public readonly float Width;

    /// <summary>
    /// Gets the height of the viewport in pixels.
    /// </summary>
    public readonly float Height;

    /// <summary>
    /// Gets the minimum depth value (default 0.0).
    /// </summary>
    public readonly float MinDepth;

    /// <summary>
    /// Gets the maximum depth value (default 1.0).
    /// </summary>
    public readonly float MaxDepth;

    /// <summary>Maps positive NDC Y upward, matching Metal and Direct3D.</summary>
    public readonly bool YUp;

    /// <summary>
    /// Creates a new viewport.
    /// </summary>
    /// <param name="x">X coordinate of upper-left corner.</param>
    /// <param name="y">Y coordinate of upper-left corner.</param>
    /// <param name="width">Width in pixels. Must be greater than 0.</param>
    /// <param name="height">Height in pixels. Must be greater than 0.</param>
    /// <param name="minDepth">Minimum depth value (default 0.0).</param>
    /// <param name="maxDepth">Maximum depth value (default 1.0).</param>
    /// <param name="yUp">Use native GPU NDC orientation (+1 at the top).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when width or height is less than or equal to 0.
    /// </exception>
    public Viewport(float x, float y, float width, float height, float minDepth = 0.0f, float maxDepth = 1.0f, bool yUp = false)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than 0");

        X = x;
        Y = y;
        Width = width;
        Height = height;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
        YUp = yUp;
    }

    /// <summary>
    /// Creates a viewport that covers the entire framebuffer.
    /// </summary>
    /// <param name="framebuffer">The framebuffer to create a viewport for.</param>
    /// <returns>A viewport covering the entire framebuffer.</returns>
    public static Viewport FromFramebuffer(Framebuffer framebuffer)
    {
        return new Viewport(0, 0, framebuffer.Width, framebuffer.Height);
    }

    /// <summary>
    /// Transforms a position from normalized device coordinates to screen space.
    /// </summary>
    /// <param name="ndcX">X coordinate in NDC space [-1, 1].</param>
    /// <param name="ndcY">Y coordinate in NDC space [-1, 1].</param>
    /// <param name="ndcZ">Z coordinate in NDC space [0, 1].</param>
    /// <param name="screenX">Output X coordinate in screen space.</param>
    /// <param name="screenY">Output Y coordinate in screen space.</param>
    /// <param name="depth">Output depth value.</param>
    /// <remarks>
    /// NDC coordinates map as follows:
    /// - X: -1 (left) to +1 (right)
    /// - Y: -1 (top) to +1 (bottom) [Note: Y is inverted from standard GL convention]
    /// - Z: 0 (near) to 1 (far)
    /// </remarks>
    public readonly void Transform(float ndcX, float ndcY, float ndcZ, out float screenX, out float screenY, out float depth)
    {
        // Transform from [-1, 1] NDC space to screen space
        // NDC: -1 is left/top, +1 is right/bottom
        screenX = X + (ndcX + 1.0f) * 0.5f * Width;
        screenY = Y + ((YUp ? -ndcY : ndcY) + 1.0f) * 0.5f * Height;

        // Transform depth from [0, 1] to depth range
        depth = MinDepth + ndcZ * (MaxDepth - MinDepth);
    }
}
