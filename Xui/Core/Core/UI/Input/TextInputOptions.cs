namespace Xui.Core.UI.Input;

/// <summary>Keyboard layout hint. Character validation remains the editor's responsibility.</summary>
public enum TextInputKeyboard
{
    /// <summary>Ordinary text in the user's language.</summary>
    Text,
    /// <summary>ASCII letters, numbers and punctuation, for values such as hex colors.</summary>
    Ascii,
    /// <summary>Digits.</summary>
    Number,
    /// <summary>Digits and a decimal separator.</summary>
    Decimal,
}

/// <summary>Semantic hints for platform suggestions and autofill.</summary>
public enum TextInputContentType
{
    /// <summary>No specific semantic hint.</summary>
    None,
    /// <summary>A person's name.</summary>
    Name,
    /// <summary>An existing password. Masking is controlled separately by IsPassword.</summary>
    Password,
}

/// <summary>An editing action from a software keyboard or its navigation controls.</summary>
public enum TextInputAction
{
    /// <summary>Use the field's configured action; otherwise finish editing.</summary>
    Default,
    /// <summary>Focus the preceding text input, without wrapping.</summary>
    Previous,
    /// <summary>Focus the next control, or finish editing at the end.</summary>
    Next,
    /// <summary>Finish editing and release keyboard focus.</summary>
    Done,
}

/// <summary>
/// Platform-independent keyboard hints, read when a field receives focus.
/// These do not restrict pasted, dictated, or hardware-keyboard input; use InputFilter for validation.
/// </summary>
public readonly record struct TextInputOptions(
    TextInputKeyboard Keyboard = TextInputKeyboard.Text,
    TextInputContentType ContentType = TextInputContentType.None,
    TextInputAction ReturnAction = TextInputAction.Default);
