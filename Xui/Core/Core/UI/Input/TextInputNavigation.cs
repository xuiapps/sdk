using Xui.Core.DI;

namespace Xui.Core.UI.Input;

/// <summary>Shared focus behavior for single-line editors and native keyboard actions.</summary>
public static class TextInputNavigation
{
    /// <summary>Whether an action is available for the currently focused editor.</summary>
    public static bool CanPerform(View view, TextInputAction action)
    {
        if (!view.IsFocused) return false;
        return action != TextInputAction.Previous
            || view.GetService<IFocus>()?.GetPrevious() is ITextInputClient;
    }

    /// <summary>Moves through focus order without wrapping, or dismisses the keyboard.</summary>
    public static void Perform(View view, TextInputAction action)
    {
        if (view is not ITextInputClient client) return;
        if (action == TextInputAction.Default) action = client.InputOptions.ReturnAction;
        if (!CanPerform(view, action)) return;
        var focus = view.GetService<IFocus>();
        switch (action)
        {
            case TextInputAction.Previous:
                if (focus?.GetPrevious() is View previous) previous.Focus();
                break;
            case TextInputAction.Next when client.InputOptions.ReturnAction != TextInputAction.Done:
                if (focus?.GetNext() is View next) next.Focus();
                else view.Blur();
                break;
            default:
                view.Blur();
                break;
        }
    }
}
