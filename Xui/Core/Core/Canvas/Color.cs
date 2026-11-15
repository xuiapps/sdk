using System.Runtime.InteropServices;

namespace Xui.Core.Canvas;

/// <summary>
/// Represents a color using red, green, blue, and alpha components, all normalized to the range [0, 1].
/// </summary>
public struct Color : IEquatable<Color>
{
    /// <inheritdoc/>
    public readonly bool Equals(Color other) =>
        Red.Equals(other.Red) && Green.Equals(other.Green) && Blue.Equals(other.Blue) && Alpha.Equals(other.Alpha);

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is Color other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

    /// <summary>
    /// Red component of the color, in the range [0, 1].
    /// </summary>
    public nfloat Red;

    /// <summary>
    /// Green component of the color, in the range [0, 1].
    /// </summary>
    public nfloat Green;

    /// <summary>
    /// Blue component of the color, in the range [0, 1].
    /// </summary>
    public nfloat Blue;

    /// <summary>
    /// Alpha (opacity) component of the color, in the range [0, 1].
    /// </summary>
    public nfloat Alpha;

    /// <summary>
    /// Initializes a new <see cref="Color"/> with the specified red, green, blue, and alpha components.
    /// </summary>
    /// <param name="red">Red component, normalized [0, 1]</param>
    /// <param name="green">Green component, normalized [0, 1]</param>
    /// <param name="blue">Blue component, normalized [0, 1]</param>
    /// <param name="alpha">Alpha component, normalized [0, 1]</param>
    public Color(nfloat red, nfloat green, nfloat blue, nfloat alpha)
    {
        this.Red = red;
        this.Green = green;
        this.Blue = blue;
        this.Alpha = alpha;
    }

    /// <summary>
    /// Initializes a new <see cref="Color"/> from byte components in the range [0, 255],
    /// which are normalized to [0, 1].
    /// </summary>
    /// <param name="red">Red component [0, 255]</param>
    /// <param name="green">Green component [0, 255]</param>
    /// <param name="blue">Blue component [0, 255]</param>
    /// <param name="alpha">Alpha component [0, 255]</param>
    public Color(byte red, byte green, byte blue, byte alpha)
    {
        this.Red   = red   / (nfloat)255;
        this.Green = green / (nfloat)255;
        this.Blue  = blue  / (nfloat)255;
        this.Alpha = alpha / (nfloat)255;
    }

    /// <summary>
    /// Initializes a new <see cref="Color"/> from a packed 32-bit RGBA value.
    /// The value format is 0xRRGGBBAA, where each component is 8 bits.
    /// </summary>
    /// <param name="rgba">Packed RGBA value in 0xRRGGBBAA format.</param>
    public Color(uint rgba)
    {
        this.Red   = ((rgba & 0xFF000000) >> 24) / (nfloat)255;
        this.Green = ((rgba & 0x00FF0000) >> 16) / (nfloat)255;
        this.Blue  = ((rgba & 0x0000FF00) >> 8)  / (nfloat)255;
        this.Alpha = ((rgba & 0x000000FF) >> 0)  / (nfloat)255;
    }

    /// <summary>
    /// Returns true if the color is fully transparent (Alpha = 0).
    /// </summary>
    public readonly bool IsTransparent => Alpha == 0;

    /// <summary>
    /// Implicitly converts a 32-bit RGBA value (0xRRGGBBAA) to a <see cref="Color"/>.
    /// </summary>
    /// <param name="rgbaHex">Packed RGBA hex value.</param>
    public static implicit operator Color(uint rgbaHex) => new Color(rgbaHex);
}
