namespace Xui.Core.Canvas.Composition.GPU;

/// <summary>A retained GPU leaf, presented by its runtime-owned surface.</summary>
/// <remarks>GPU nodes have no canvas command buffers, inherited canvas paint, or children.
/// A platform can derive from this type to own its native presentation container.</remarks>
public class GpuNode : Node
{
    /// <summary>The runtime-owned GPU surface, created on first declaration.</summary>
    public ICompositionGpuSurface? Surface { get; internal set; }

    /// <inheritdoc/>
    protected override void DisposeCore()
    {
        Surface?.Dispose();
        Surface = null;
        base.DisposeCore();
    }
}
