using Xui.Core.DI;

namespace Xui.Core.UI;

public partial class View
{
    /// <summary>
    /// Called when this view receives keyboard focus.
    /// </summary>
    protected internal virtual void OnFocus() { }

    /// <summary>
    /// Called when this view loses keyboard focus.
    /// </summary>
    protected internal virtual void OnBlur() { }

    /// <summary>
    /// Gets whether this view can receive keyboard focus via Tab navigation.
    /// </summary>
    public virtual bool Focusable => false;

    /// <summary>
    /// Gets whether this view currently has keyboard focus.
    /// </summary>
    public bool IsFocused => this.GetService<IFocus>()?.FocusedView == this;

    /// <summary>
    /// Requests keyboard focus for this view.
    /// </summary>
    public bool Focus()
    {
        var focus = this.GetService<IFocus>();
        if (focus == null)
            return false;

        focus.FocusedView = this;
        return true;
    }

    /// <summary>
    /// Releases keyboard focus from this view.
    /// </summary>
    public void Blur()
    {
        var focus = this.GetService<IFocus>();
        if (focus?.FocusedView == this)
            focus.FocusedView = null;
    }
}
