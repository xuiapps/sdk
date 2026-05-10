namespace Xui.Core.UI.Input
{
    /// <summary>
    /// A capturing view declares <see cref="ITap"/> when it is tracking a press
    /// that may or may not become a tap (e.g. <c>Button</c>, <c>Checkbox</c>).
    /// An ancestor that wants the pointer for a drag may steal capture after its
    /// own threshold is exceeded.
    /// </summary>
    public interface ITap : IPointerGesture
    {
    }
}
