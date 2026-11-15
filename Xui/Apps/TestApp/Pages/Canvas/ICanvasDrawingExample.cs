using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Apps.TestApp.Pages.Canvas;

/// <summary>Artwork bounds in the examples' original 600×300 drawing coordinates.</summary>
internal interface ICanvasDrawingExample
{
    Rect GetArtworkBounds(IMeasureContext context);
}
