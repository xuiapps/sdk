namespace Xui.Core.Canvas.Composition.GPU;

/// <summary>Runtime-owned presentation resource for a retained GPU node.</summary>
public interface ICompositionGpuSurface : IDisposable
{
    /// <summary>Whether the surface already holds a successfully presented frame.</summary>
    bool HasContent { get; }

    /// <summary>Schedules drawing after native layout; the runtime owns frame acquisition and presentation.</summary>
    void RequestRender(IGpuSurfaceContent content);
}
