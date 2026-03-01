namespace Xui.Core.UI.Layers;

public interface ILayer
{
    /// <summary>
    /// Returns <c>true</c> when this layer contributes nothing to layout or rendering.
    /// Only <see cref="Empty"/> returns <c>true</c>; all other layers return the default <c>false</c>.
    /// Stack layers use this to skip empty slots without inspecting the type at runtime.
    /// </summary>
    bool IsEmpty => false;

    public LayoutGuide Update(LayoutGuide guide);
}
