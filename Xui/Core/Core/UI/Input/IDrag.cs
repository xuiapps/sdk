namespace Xui.Core.UI.Input
{
    /// <summary>
    /// A capturing view declares <see cref="IDrag"/> when it is consuming pointer
    /// motion as ongoing input (e.g. a color wheel, a slider thumb, a drawing
    /// canvas). Ancestor scroll views and similar containers must not steal
    /// capture while an <see cref="IDrag"/> gesture is active.
    /// </summary>
    public interface IDrag : IPointerGesture
    {
    }
}
