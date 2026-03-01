// File: Xui/Core/UI/Layers/LayerExtensions.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// Extension methods that expose individual layout passes on any <see cref="ILayer"/> struct,
/// mirroring the per-pass helpers available on <see cref="View"/>
/// (<c>Animate</c>, <c>Measure</c>, <c>Arrange</c>, <c>Render</c>).
/// Each method constructs the appropriate <see cref="LayoutGuide"/> and delegates to
/// <see cref="ILayer.Update"/>.
/// </summary>
/// <remarks>
/// All methods accept the layer by <c>ref</c> so that any state written back into the layer
/// struct during a pass (e.g. caching a desired size or frame) is preserved in the caller's
/// storage without an extra copy.
/// </remarks>
public static class LayerExtensions
{
    /// <summary>Advances the layer's animation state for the current frame.</summary>
    public static void Animate<T>(this ref T layer, TimeSpan previousTime, TimeSpan currentTime)
        where T : struct, ILayer
        => layer.Update(new LayoutGuide
        {
            Pass         = LayoutGuide.LayoutPass.Animate,
            PreviousTime = previousTime,
            CurrentTime  = currentTime,
        });

    /// <summary>
    /// Measures the layer within the given available size and returns its desired size.
    /// </summary>
    public static Size Measure<T>(this ref T layer, Size available, IMeasureContext context)
        where T : struct, ILayer
    {
        var g = new LayoutGuide
        {
            Pass           = LayoutGuide.LayoutPass.Measure,
            AvailableSize  = available,
            MeasureContext = context,
        };
        g = layer.Update(g);
        return g.DesiredSize;
    }

    /// <summary>Arranges the layer within the given rectangle.</summary>
    public static void Arrange<T>(this ref T layer, Rect rect, IMeasureContext context)
        where T : struct, ILayer
        => layer.Update(new LayoutGuide
        {
            Pass           = LayoutGuide.LayoutPass.Arrange,
            ArrangedRect   = rect,
            AvailableSize  = rect.Size,
            MeasureContext = context,
        });

    /// <summary>Renders the layer within the given rectangle.</summary>
    public static void Render<T>(this ref T layer, Rect rect, IContext context)
        where T : struct, ILayer
        => layer.Update(new LayoutGuide
        {
            Pass          = LayoutGuide.LayoutPass.Render,
            ArrangedRect  = rect,
            MeasureContext = context,
            RenderContext  = context,
        });
}
