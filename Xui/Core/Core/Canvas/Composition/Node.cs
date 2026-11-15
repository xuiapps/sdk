using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition;

/// <summary>A platform-independent retained composition element with identity, placement and children.</summary>
/// <remarks>
/// <para>
/// Applications declare composition through ICompositionContext.Group and GpuSurface. A retained
/// context reconciles those declarations by key into nodes. A platform presenter maps each node
/// to suitable resources: native views or layers, compositor visuals, GPU surfaces, or drawing
/// replayed into a parent. A node does not require a native view or an intermediate bitmap.
/// Content can remain unchanged while its placement and presentation are updated.
/// </para>
/// <para>
/// Immediate and export contexts can implement the same declarations without constructing this
/// tree. SVG currently emits groups directly. iOS presents the retained tree using UIKit views,
/// effect views and MTKView GPU surfaces. Android presents backed graphics groups through Java
/// views and hardware display lists, with Vulkan presentation for GPU leaves. Windows currently draws groups immediately. Native visuals or offscreen GPU textures are possible
/// backend strategies, not capabilities guaranteed by this base class. GPU surface support and
/// the need for a portable fallback are reported by ICompositionContext.GpuSurface.
/// </para>
/// <para>
/// This is a rendering/composition model, not the Xui UI view tree. It does not supply layout,
/// input routing, focus or accessibility semantics merely because a backend uses native views.
/// Derived nodes own their content, children and any platform resources, released by overriding
/// DisposeCore. The base provides presentation properties and an idempotent disposal entry point.
/// </para>
/// </remarks>
public abstract partial class Node : IDisposable
{
    private bool disposed;

    /// <summary>Stable sibling identity, effects, clipping and content-update declarations.</summary>
    public GroupOptions Options { get; internal set; }

    /// <summary>Resolved content bounds used for drawing or native surface placement.</summary>
    public Rect RasterBounds { get; internal set; }

    /// <summary>Resolved transform relative to the parent presentation surface.</summary>
    public AffineTransform Placement { get; internal set; } = AffineTransform.Identity;

    /// <summary>Device scale used to present the content.</summary>
    public nfloat Scale { get; internal set; }

    /// <summary>Whether this frame requested fresh content for this node.</summary>
    public bool Updating { get; internal set; }

    internal long Seen;
    internal bool Valid;
    internal virtual void Prune(long frame) { }
    internal virtual void Accept() { Valid = true; Updating = false; }
    internal virtual void Invalidate() { Valid = false; }

    /// <summary>Releases the node's owned resources once, including derived content and children.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>Overrides release their owned content, children and platform resources, then call base.</summary>
    protected virtual void DisposeCore() { }
}
