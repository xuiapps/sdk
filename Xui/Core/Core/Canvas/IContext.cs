namespace Xui.Core.Canvas;

/// <summary>
/// Represents a 2D drawing context for the Xui Canvas, modeled after the HTML5 Canvas 2D context API.
/// This interface aggregates all sub-contexts responsible for different aspects of 2D rendering,
/// including state, drawing primitives, text, images, transformations, and resource management.
/// </summary>
public interface IContext :
    IMeasureContext,
    IStateContext,             // Handles save/restore state stack and global properties
    IFilterContext,            // Adjusts source colors when selecting paints
    IPenContext,               // Controls stroke/fill styles, line width, caps, joins, etc.
    IPathDrawingContext,       // Handles path creation and stroking/filling
    IRectDrawingContext,       // Shortcut methods for drawing and clearing rectangles
    ITextDrawingContext,       // Text rendering, alignment, font settings
    IImageDrawingContext,      // Drawing images, bitmaps, or image-like data
    ITransformContext,         // Transform matrix manipulation (translate, scale, rotate, etc.)
    IDisposable                // Ensures proper cleanup of native resources
{
}
