namespace Xui.Core.Canvas.Composition.GPU;

/// <summary>Runtime factory for native GPU composition surfaces owned by a window.</summary>
public interface IGpuSurfaceFactory
{
    /// <summary>Whether native GPU presentation is available.</summary>
    bool IsSupported { get; }

    /// <summary>Creates a surface whose lifetime belongs to the retained composition tree.</summary>
    /// <exception cref="NotSupportedException">A native GPU surface cannot be created on this device.</exception>
    ICompositionGpuSurface CreateSurface();
}
