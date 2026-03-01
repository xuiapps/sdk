// File: Xui/Core/UI/Layers/LayerView.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A <see cref="View"/> whose content is driven by a layer tree of type <typeparamref name="T"/>.
/// All layout passes are forwarded to <see cref="Layer"/> via <see cref="LayerExtensions"/>
/// helpers, one pass at a time. The standard <see cref="View.Update"/> flow is preserved:
/// base handles margin, alignment, and <see cref="View.Frame"/> assignment, then delegates
/// to the <c>*Core</c> overrides here.
/// </summary>
/// <remarks>
/// <para>
/// <b>Subclassing.</b> Override <see cref="RenderCore"/> to fill a background, sync state,
/// or apply padding before calling <c>base.RenderCore(context)</c> — or call
/// <c>Layer.Render(paddedRect, context)</c> directly with a custom rect.
/// </para>
/// <para>
/// <b>Single-pass optimisation.</b> For containers where all layout information is available
/// upfront (e.g. fixed-size cells), override <see cref="View.Update"/> and call
/// <c>Layer.Update(guide)</c> directly to run all active passes in one DFS.
/// This optimisation is left to subclasses; <see cref="LayerView{T}"/> itself always
/// uses the forked per-pass path through <c>base.Update</c>.
/// </para>
/// <para>
/// <b>Mixing Views and layers.</b> Embed full <see cref="View"/> subtrees inside the layer
/// tree via <see cref="ViewLayer{TView}"/> rather than adding child views to the
/// <see cref="View"/> child collection.
/// </para>
/// </remarks>
/// <typeparam name="T">Root layer struct type (value type implementing <see cref="ILayer"/>).</typeparam>
public class LayerView<T> : View
    where T : struct, ILayer
{
    /// <summary>
    /// Root of the layer tree. Because this is a plain field on a heap-allocated class,
    /// nested field mutations such as <c>Layer.Item2.Left.Checked = true</c> are in-place
    /// and do not require a copy-back.
    /// </summary>
    protected T Layer;

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
        => Layer.Animate(previousTime, currentTime);

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
        => Layer.Measure(available, context);

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
        => Layer.Arrange(rect, context);

    /// <inheritdoc/>
    /// <remarks>
    /// Renders the layer tree into <see cref="View.Frame"/>, which is set by
    /// <c>base.Update</c>'s Arrange pass before this method is called.
    /// Override to fill a background or apply padding before/instead-of calling base.
    /// </remarks>
    protected override void RenderCore(IContext context)
        => Layer.Render(Frame, context);
}
