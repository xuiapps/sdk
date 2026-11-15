using Xui.Core.UI.Input;

namespace Xui.Core.UI;

/// <summary>
/// Basic text-entry capabilities of a focusable view. Focus requests the software
/// keyboard; the responder can query these traits without copying the text buffer.
/// This does not imply support for marked text, IME composition, or native range editing.
/// </summary>
public interface ITextInputClient
{
    /// <summary>Whether the input contains any text.</summary>
    bool HasText { get; }

    /// <summary>Whether the input is a masked password field.</summary>
    bool IsPassword { get; }

    /// <summary>Keyboard layout, content semantics and return-key action.</summary>
    TextInputOptions InputOptions { get; }

    /// <summary>Whether a keyboard action is currently available.</summary>
    bool CanPerformInputAction(TextInputAction action);

    /// <summary>Handles a keyboard action, including focus navigation or finishing editing.</summary>
    void PerformInputAction(TextInputAction action);
}
