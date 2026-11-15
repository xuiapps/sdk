using System.Runtime.InteropServices;
using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.Runtime.IOS;

/// <summary>Maps shared editor hints to UIKit's UITextInputTraits values.</summary>
internal static class UITextInputTraits
{
    internal static nint KeyboardType(TextInputOptions options) => options.Keyboard switch
    {
        TextInputKeyboard.Ascii => 1, // UIKeyboardTypeASCIICapable
        TextInputKeyboard.Number => 4, // UIKeyboardTypeNumberPad
        TextInputKeyboard.Decimal => 8, // UIKeyboardTypeDecimalPad
        _ => 0,
    };

    internal static nint ReturnKeyType(TextInputOptions options) => options.ReturnAction switch
    {
        TextInputAction.Next => 4, // UIReturnKeyNext
        _ => 9, // UIReturnKeyDone (single-line editing)
    };

    internal static nint ContentType(ITextInputClient? client)
    {
        string? symbol = client?.IsPassword == true ? "UITextContentTypePassword" : client?.InputOptions.ContentType switch
        {
            TextInputContentType.Name => "UITextContentTypeName",
            TextInputContentType.Password => "UITextContentTypePassword",
            _ => null,
        };
        return symbol is null ? 0 : Marshal.ReadIntPtr(NativeLibrary.GetExport(UIKit.Lib, symbol));
    }

    internal static bool PlainText(ITextInputClient? client) => client is { IsPassword: false }
        && client.InputOptions.Keyboard == TextInputKeyboard.Text
        && client.InputOptions.ContentType != TextInputContentType.Password;

    internal static nint Capitalization(ITextInputClient? client) => !PlainText(client) ? 0
        : client!.InputOptions.ContentType == TextInputContentType.Name ? 1 : 2;
}
