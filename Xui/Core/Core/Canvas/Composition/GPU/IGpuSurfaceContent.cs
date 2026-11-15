using Xui.GPU.Hardware;

namespace Xui.Core.Canvas.Composition.GPU;

/// <summary>A stable reference used both as a GPU surface's key and as its renderer.</summary>
/// <remarks>Implement on a class, typically the View that owns the scene. The window calls
/// Render on its UI thread after placing the native surface. Do not retain or dispose the
/// borrowed target; encode and commit GPU commands during this call. Bounds are supplied
/// by GroupOptions; target dimensions are physical pixels. Canvas paint is not inherited.</remarks>
public interface IGpuSurfaceContent
{
    /// <summary>Draws and submits one frame into the borrowed presentation target.</summary>
    void Render(IGpuRenderTarget target);
}
