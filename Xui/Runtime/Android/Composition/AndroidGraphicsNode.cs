using Xui.Core.Canvas.Composition.Graphics;

namespace Xui.Runtime.Android.Composition;

internal sealed class AndroidGraphicsNode(int id, Action<int> release) : GraphicsNode
{
    internal int Id { get; } = id;
    protected override void DisposeCore() { release(Id); base.DisposeCore(); }
}
