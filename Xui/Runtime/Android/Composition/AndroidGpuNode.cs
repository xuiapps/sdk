using Xui.Core.Canvas.Composition.GPU;
namespace Xui.Runtime.Android.Composition;

internal sealed class AndroidGpuNode(int id,Action<int> release) : GpuNode
{
    internal int Id { get; } = id;
    protected override void DisposeCore() { release(Id);base.DisposeCore(); }
}
