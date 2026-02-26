using Xui.Core.Abstract.Events;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

public partial class View
{
    /// <summary>
    /// Called during event dispatch to handle a pointer event in a specific event phase.
    /// </summary>
    public virtual void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        // override in child classes
    }

    /// <summary>
    /// Called when a scroll wheel or trackpad scroll event is dispatched to this view.
    /// Override to handle scroll input. Set <see cref="ScrollWheelEventRef.Handled"/> to stop propagation.
    /// </summary>
    public virtual void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        // override in subclasses (e.g. ScrollView)
    }

    /// <summary>
    /// Called when a key is pressed while this view has focus.
    /// </summary>
    public virtual void OnKeyDown(ref KeyEventRef e)
    {
    }

    /// <summary>
    /// Called when a character is input while this view has focus.
    /// </summary>
    public virtual void OnChar(ref KeyEventRef e)
    {
    }

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
    public bool IsFocused
    {
        get
        {
            var rootView = this.FindRootView();
            return rootView?.FocusedView == this;
        }
    }

    /// <summary>
    /// Requests keyboard focus for this view.
    /// </summary>
    public bool Focus()
    {
        var rootView = this.FindRootView();
        if (rootView == null)
            return false;

        rootView.FocusedView = this;
        return true;
    }

    /// <summary>
    /// Releases keyboard focus from this view.
    /// </summary>
    public void Blur()
    {
        var rootView = this.FindRootView();
        if (rootView?.FocusedView == this)
            rootView.FocusedView = null;
    }

    private RootView? FindRootView()
    {
        View? current = this;
        while (current != null)
        {
            if (current is RootView root)
                return root;
            current = current.Parent;
        }
        return null;
    }
}
