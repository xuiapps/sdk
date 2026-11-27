namespace Xui.Core.Canvas.Composition.GPU;

/// <summary>Why a context could not present a native GPU surface.</summary>
public enum GpuSurfaceFailure
{
    /// <summary>The native GPU surface was accepted.</summary>
    None,
    /// <summary>
    /// This drawing context requires portable output, for example an SVG capture or
    /// a native context under an unsupported clip. An offscreen fallback may still work.
    /// </summary>
    PortableFallbackRequired,
    /// <summary>The runtime could not create a GPU device or native GPU surface.</summary>
    NotSupported
}
