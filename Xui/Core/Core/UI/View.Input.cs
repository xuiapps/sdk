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
}
