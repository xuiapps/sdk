namespace Xui.Core.UI.Layers;

public interface IContainer<TChild> : ILayer
    where TChild : struct, ILayer
{
}
