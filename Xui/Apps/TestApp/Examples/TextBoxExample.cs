using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.Apps.TestApp.Examples;

public class TextBoxExample : Example
{
    public TextBoxExample()
    {
        Title = "Text Input";
        Content = new VerticalStack
        {
            Margin = 8,
            Content =
            [
                new Label { Text = "Name:", FontFamily = "Inter", Margin = (4, 2) },
                new TextBox { Id = "NameBox", Text = "", FontFamily = "Inter", Margin = (4, 2),
                    InputOptions = new(ContentType: TextInputContentType.Name, ReturnAction: TextInputAction.Next) },

                new Label { Text = "Password:", FontFamily = "Inter", Margin = (4, 2) },
                new TextBox { Id = "PasswordBox", Text = "", IsPassword = true, FontFamily = "Inter", Margin = (4, 2),
                    InputOptions = new(ContentType: TextInputContentType.Password, ReturnAction: TextInputAction.Next) },

                new Label { Text = "Number:", FontFamily = "Inter", Margin = (4, 2) },
                new TextBox { Id = "NumberBox", Text = "0", FontFamily = "Inter", Margin = (4, 2),
                    InputOptions = new(Keyboard: TextInputKeyboard.Decimal, ReturnAction: TextInputAction.Next),
                    InputFilter = c => char.IsAsciiDigit(c) || c == '.' },

                new Label { Text = "Color (hex):", FontFamily = "Inter", Margin = (4, 2) },
                new TextBox { Id = "ColorBox", Text = "#FF0000", FontFamily = "Inter", Margin = (4, 2),
                    InputOptions = new(Keyboard: TextInputKeyboard.Ascii, ReturnAction: TextInputAction.Done),
                    InputFilter = c => char.IsAsciiHexDigit(c) || c == '#' },
            ]
        };
    }
}
