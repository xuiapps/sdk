using Xui.Core.Abstract.Events;
using Xui.Core.UI;
using Xui.Core.UI.Input;

// Platform input enters the real Window and shared TextEditingState.
public static partial class Program
{
    public static object TextClient(BrowserSession session) => session.Window.RootView.FocusedView is ITextEditingClient client ? client : null;
    public static string TextValue(BrowserSession session) => session.Window.RootView.FocusedView is ITextEditingClient client ? client.Editor.Text : "";
    public static int TextStart(BrowserSession session) => session.Window.RootView.FocusedView is ITextEditingClient client ? client.Editor.Selection.Start : 0;
    public static int TextEnd(BrowserSession session) => session.Window.RootView.FocusedView is ITextEditingClient client ? client.Editor.Selection.End : 0;
    public static int TextSecure(BrowserSession session) => session.Window.IsSecureTextEntry ? 1 : 0;
    public static void TextSelect(BrowserSession session, int start, int end)
    {
        if (session.Window.RootView.FocusedView is ITextEditingClient client) client.Editor.Selection = new TextRange(start, end);
    }
    public static void TextInsert(BrowserSession session, string text)
    {
        var input = new InsertTextEventRef(text); session.Window.InsertText(ref input);
    }
    private static string textError;
    public static string TextError() => textError;
    public static int TextKey(BrowserSession session, int key, int shift, int repeat)
    {
        var input = new KeyEventRef { Key=(VirtualKey)key, Shift=shift!=0, IsRepeat=repeat!=0 };
        textError = null;
        try { session.Window.OnKeyDown(ref input); return input.Handled ? 1 : 0; }
        catch (System.Exception error) { textError = error.Message; throw; }
    }
}
