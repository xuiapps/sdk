using Xui.Core.Math2D;

namespace Xui.Core.Canvas;

/// <summary>
/// Defines methods for drawing bitmap images onto the canvas.
/// Mirrors the <c>drawImage</c> API from the HTML5 Canvas 2D context.
/// https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/drawImage
/// </summary>
public interface IImageDrawingContext
{
    /// <summary>
    /// Draws the entire <paramref name="image"/> scaled to fit <paramref name="dest"/> at full opacity.
    /// </summary>
    void DrawImage(Bitmap image, Rect dest);

    /// <summary>
    /// Draws the entire <paramref name="image"/> scaled to fit <paramref name="dest"/>
    /// at the given <paramref name="opacity"/>.
    /// </summary>
    void DrawImage(Bitmap image, Rect dest, nfloat opacity);

    /// <summary>
    /// Draws the sub-region <paramref name="source"/> of <paramref name="image"/>
    /// scaled to fit <paramref name="dest"/> at the given <paramref name="opacity"/>.
    /// Mirrors the 9-argument form of <c>drawImage</c> in the HTML5 Canvas API.
    /// </summary>
    void DrawImage(Bitmap image, Rect source, Rect dest, nfloat opacity);

    /// <summary>Draws the entire image into the destination rectangle.</summary>
    void DrawImage(IImage image, Rect destination);

    /// <summary>Draws a cropped region of the image into the destination rectangle.</summary>
    void DrawImage(IImage image, Rect source, Rect destination);
}
