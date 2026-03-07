using Xui.Core.Math2D;

namespace Xui.Core.UI.Layer;

/// <summary>
/// The contract a host view must satisfy to be used with <see cref="ILayer{TView}"/>.
/// Provides the invalidation, focus, pointer-capture, and service-resolution APIs
/// that layers call on their host during layout, rendering, and input handling.
/// </summary>
public interface ILayerHost : IServiceProvider
{
    /// <summary>The arranged frame of the host view, in parent-local coordinates.</summary>
    Rect Frame { get; }

    /// <summary>Whether the host view currently holds keyboard focus.</summary>
    bool IsFocused { get; }

    /// <summary>Requests keyboard focus for the host view. Returns true if focus was granted.</summary>
    bool Focus();

    /// <summary>Marks the view as needing re-render.</summary>
    void InvalidateRender();

    /// <summary>Marks the view as needing re-measure (and re-layout).</summary>
    void InvalidateMeasure();

    /// <summary>Schedules an animation frame callback via <c>AnimateCore</c>.</summary>
    void RequestAnimationFrame();

    /// <summary>Routes all subsequent pointer events for <paramref name="pointerId"/> to this view.</summary>
    void CapturePointer(int pointerId);

    /// <summary>Releases a previously captured pointer.</summary>
    void ReleasePointer(int pointerId);
}
