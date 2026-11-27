using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;

namespace Xui.Core.Canvas;

/// <summary>Declares explicit groups in the drawing order of a canvas.</summary>
public interface ICompositionContext
{
    /// <summary>
    /// Opens a group and applies its presentation properties. Draw through the same context
    /// only when the returned scope's <see cref="CompositionGroup.NeedsRedraw"/> is true.
    /// Dispose the scope before drawing subsequent siblings.
    /// </summary>
    /// <remarks>
    /// Keys identify siblings by reference, not value equality. Use a stable reference, not
    /// a boxed value or a newly allocated key each frame. HasUpdates=false permits reuse;
    /// it never guarantees it. Export and immediate contexts always request drawing.
    /// A retained backend must also redraw when it lacks valid content. Reusing a group
    /// reuses its entire subtree, so HasUpdates must include relevant descendant changes.
    /// Presentation changes are processed even when content can be reused.
    /// HasUpdates=true must request drawing; it is also used by capture middleware to
    /// force both the display and export branches to accept the same drawing commands.
    /// Groups inherit drawing state and restore it on disposal, with the same path and
    /// state semantics as Save/Restore. Bounds do not change the coordinate origin.
    /// In a view-backed group, issue direct drawing before declaring child surfaces.
    /// To draw above those children, declare a separate overlay group. Retained backends
    /// reject drawing that violates this ordering instead of silently changing its depth.
    /// An inherited arbitrary canvas clip may require flattening a surface into its parent;
    /// that fallback requests drawing on every frame. Prefer group bounds clipping when
    /// native composition and content reuse are needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// using var group = context.Group(new GroupOptions
    /// {
    ///     Key = this,
    ///     HasUpdates = HasRenderUpdates,
    ///     Bounds = Frame
    /// });
    /// if (group.NeedsRedraw)
    ///     DrawContent(context);
    /// </code>
    /// </example>
    CompositionGroup Group(scoped in GroupOptions options);

    /// <summary>
    /// Declares a native GPU leaf in drawing order. Key must be a stable class implementing
    /// IGpuSurfaceContent; Bounds is required. Rendering is deferred until native placement,
    /// and requested when HasUpdates is true or the retained surface needs fresh content.
    /// Available from a Window render context as well as a View render context.
    /// </summary>
    /// <returns>True when native presentation is supported here; false requests an explicit
    /// application fallback (for example an offscreen image for SVG export). A false return
    /// does not change composition state. Arbitrary inherited canvas clips are unsupported;
    /// use group bounds clipping. Native GPU content does not inherit canvas paint/filter.</returns>
    /// <example>
    /// A Window can declare the surface directly from its RenderEventRef.Context:
    /// <code>
    /// context.GpuSurface(new GroupOptions
    /// {
    ///     Key = cube, // cube implements IGpuSurfaceContent
    ///     Bounds = displayArea,
    ///     HasUpdates = cubeChanged
    /// });
    /// </code>
    /// Check the returned bool and render a portable fallback when false.
    /// </example>
    bool GpuSurface(scoped in GroupOptions options)
    {
        ValidateGpuSurface(in options);
        return false;
    }

    /// <summary>
    /// Attempts native GPU presentation and reports why it could not be accepted.
    /// Use <see cref="GpuSurfaceFailure.NotSupported"/> to display an unavailable message;
    /// <see cref="GpuSurfaceFailure.PortableFallbackRequired"/> permits an explicit export
    /// or offscreen fallback. Invalid options and rendering/shader errors still throw.
    /// </summary>
    /// <remarks>Older contexts preserve their existing portable-fallback behavior.</remarks>
    bool GpuSurface(scoped in GroupOptions options, out GpuSurfaceFailure failure)
    {
        var accepted = GpuSurface(in options);
        failure = accepted ? GpuSurfaceFailure.None : GpuSurfaceFailure.PortableFallbackRequired;
        return accepted;
    }

    /// <summary>Validates GPU identity and bounds before a backend changes state.</summary>
    static void ValidateGpuSurface(scoped in GroupOptions options)
    {
        options.Validate();
        if (options.Key is not IGpuSurfaceContent)
            throw new ArgumentException("A GPU surface key must implement IGpuSurfaceContent.", nameof(options));
        if (options.Bounds is null)
            throw new ArgumentException("A GPU surface requires bounds.", nameof(options));
    }

    /// <summary>
    /// Closes the innermost group. Called by <see cref="CompositionGroup.Dispose"/>;
    /// prefer disposing the returned scope over calling this directly.
    /// </summary>
    void EndGroup();
}
