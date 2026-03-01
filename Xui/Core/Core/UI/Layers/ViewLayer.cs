// File: Xui/Core/UI/Layers/ViewLayer.cs
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A layer struct that wraps a <see cref="View"/> reference, bridging from a layer tree
/// back into the View hierarchy. All <see cref="ILayer.Update"/> calls are forwarded to
/// the wrapped view's <see cref="View.Update"/> method.
/// </summary>
/// <remarks>
/// Use this inside a <see cref="LayerView{T}"/> subclass when you need full
/// <see cref="View"/> subtrees (e.g. text input, video, embedded UI) as named slots
/// inside a layer tree, without adding them to the parent's child-view collection.
/// The wrapped view participates in Animate, Measure, Arrange, and Render as if it
/// were a first-class child — the layer system calls <see cref="View.Update"/> directly.
/// </remarks>
/// <typeparam name="TView">Concrete <see cref="View"/> type being wrapped.</typeparam>
public struct ViewLayer<TView> : ILayer
    where TView : View
{
    /// <summary>The wrapped view. When <c>null</c> this slot is treated as empty.</summary>
    public TView? View;

    /// <inheritdoc/>
    public bool IsEmpty => View is null;

    /// <inheritdoc/>
    public LayoutGuide Update(LayoutGuide guide) => View is null ? guide : View.Update(guide);
}
