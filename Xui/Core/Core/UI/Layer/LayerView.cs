using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// A <see cref="View"/> that owns a single <typeparamref name="TLayer"/> struct and delegates
/// every layout, rendering, and input override to it, passing itself as the fully-typed
/// <typeparamref name="TView"/> host to every layer method.
/// <para>
/// Because the compiler resolves <typeparamref name="TLayer"/> statically, all delegation is
/// devirtualised and inlined by the JIT — zero overhead compared to writing the overrides by hand.
/// </para>
/// <para>
/// <b>Child views:</b> This class is a leaf by default (Count = 0).
/// Subclasses that host child <see cref="View"/> instances should override <see cref="View.Count"/>
/// and <see cref="View.this[int]"/>, and manage attachment via <see cref="View.SetProtectedChild{T}"/>.
/// Use <see cref="ContentLayer"/> to bridge those views back into the layer tree.
/// </para>
/// </summary>
/// <typeparam name="TView">The concrete view subclass. Must be the class that inherits this.
/// Pass <see cref="View"/> when no specific host type is needed.</typeparam>
/// <typeparam name="TLayer">The layer struct type.</typeparam>
public class LayerView<TView, TLayer> : View
    where TView  : View
    where TLayer : struct, ILayer<TView>
{
    /// <summary>The layer struct that owns all layout and rendering logic for this view.</summary>
    protected TLayer Layer;

    private TView Self => (TView)(object)this;

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
        => Layer.Animate(Self, previousTime, currentTime);

    /// <inheritdoc/>
    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
        => Layer.Measure(Self, availableSize, context);

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
        => Layer.Arrange(Self, rect, context);

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
        => Layer.Render(Self, context);

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        => Layer.OnPointerEvent(Self, ref e, phase);

    /// <inheritdoc/>
    public override void OnKeyDown(ref KeyEventRef e)
        => Layer.OnKeyDown(Self, ref e);

    /// <inheritdoc/>
    public override void OnChar(ref KeyEventRef e)
        => Layer.OnChar(Self, ref e);

    /// <inheritdoc/>
    protected internal override void OnFocus()
        => Layer.OnFocus(Self);

    /// <inheritdoc/>
    protected internal override void OnBlur()
        => Layer.OnBlur(Self);
}
