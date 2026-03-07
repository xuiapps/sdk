using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// A composable struct that handles a portion of a view's layout, rendering, and input.
/// Layers are value types composed at compile time — no virtual dispatch, no allocations.
/// The host <typeparamref name="TView"/> provides resources and invalidation callbacks;
/// the layer carries its own state and logic.
/// </summary>
/// <typeparam name="TView">
/// The host type for this layer. Must implement <see cref="ILayerHost"/>.
/// Almost always <see cref="View"/>; use a more derived type only when the layer
/// needs access to host-specific APIs beyond the <see cref="ILayerHost"/> contract.
/// </typeparam>
public interface ILayer<in TView>
    where TView : ILayerHost
{
    /// <summary>
    /// Drives one or more layout passes. Mirrors <see cref="View.Update"/>.
    /// The default per-pass methods are preferred for clarity; override this only when a
    /// single-DFS LuminarFlow traversal is needed (leaf layers or single-child-stretch containers).
    /// </summary>
    void Update(TView view, ref LayoutGuide guide);

    /// <summary>Advances time-based state (animations, blinking cursors, etc.).</summary>
    void Animate(TView view, TimeSpan previousTime, TimeSpan currentTime);

    /// <summary>Returns the desired border-edge size given the available space.</summary>
    Size Measure(TView view, Size availableSize, IMeasureContext context);

    /// <summary>Finalises position and size within <paramref name="rect"/>.</summary>
    void Arrange(TView view, Rect rect, IMeasureContext context);

    /// <summary>Draws content. Called after Arrange.</summary>
    void Render(TView view, IContext context);

    /// <summary>Handles a pointer event in the given dispatch phase.</summary>
    void OnPointerEvent(TView view, ref PointerEventRef e, EventPhase phase);

    /// <summary>Handles a key-down event while the host view has focus.</summary>
    void OnKeyDown(TView view, ref KeyEventRef e);

    /// <summary>Handles a character input event while the host view has focus.</summary>
    void OnChar(TView view, ref KeyEventRef e);

    /// <summary>Called when the host view gains keyboard focus.</summary>
    void OnFocus(TView view);

    /// <summary>Called when the host view loses keyboard focus.</summary>
    void OnBlur(TView view);
}
