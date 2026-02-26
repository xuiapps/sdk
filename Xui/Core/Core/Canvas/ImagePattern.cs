namespace Xui.Core.Canvas;

/// <summary>
/// Describes an image-based fill or stroke pattern — the Canvas API equivalent of a
/// <c>CanvasPattern</c> returned by <c>createPattern(image, repetition)</c>.
/// <para>
/// Pass to <see cref="IPenContext.SetFill(ImagePattern)"/> or
/// <see cref="IPenContext.SetStroke(ImagePattern)"/> to paint paths with a tiled image.
/// </para>
/// </summary>
public ref struct ImagePattern
{
    /// <summary>The image to tile.</summary>
    public IImage Image;

    /// <summary>How the image repeats beyond its natural bounds.</summary>
    public PatternRepeat Repetition;

    /// <param name="image">The source image (must be loaded before drawing).</param>
    /// <param name="repetition">Tiling mode; defaults to <see cref="PatternRepeat.Repeat"/>.</param>
    public ImagePattern(IImage image, PatternRepeat repetition = PatternRepeat.Repeat)
    {
        Image = image;
        Repetition = repetition;
    }
}
