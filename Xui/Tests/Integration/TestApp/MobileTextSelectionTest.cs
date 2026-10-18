using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class MobileTextSelectionTest
{
    [Fact]
    public void TextInputPageUsesTouchMeasurementKeyboardAndNativePresentationService()
    {
        var magnifier = new RecordingMagnifier();
        using var app = new TestSinglePageApp<Application, MainWindow>((441, 916),
            configure: services =>
            {
                services.AddSingleton<ITestSettings>(new TestSettings());
                services.AddSingleton<ITextMagnifier>(magnifier);
            }, runtimeVariant: TestRuntimeVariant.IPhoneEmulator,
            fontSource: Xui.Core.Fonts.Inter.Source);
        app.Render();
        var button = Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(app.Window.RootView.FindViewById("TextBox"));
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        // The Input category is below the home viewport. Open through its real action
        // and finish navigation before interacting with the page.
        button.Clicked!();
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(350));
        app.Render();

        var name = Assert.IsType<TextBox>(app.Window.RootView.FindViewById("NameBox"));
        var password = Assert.IsType<TextBox>(app.Window.RootView.FindViewById("PasswordBox"));
        // The diagnostics overlay used to intercept the right half, even while hidden.
        var rightSide = name.LocalToGlobal((name.Frame.Right - 12, name.Frame.Center.Y));
        Assert.True(navigation.OverlayContent!.Frame.Contains(navigation.OverlayContent.GlobalToLocal(rightSide)));
        app.PointerDown(rightSide);
        app.PointerUp(rightSide);
        Assert.True(name.IsFocused);
        Assert.Same(magnifier, name.GetService(typeof(ITextMagnifier)));
        name.Focus();
        var input = new InsertTextEventRef("Precision selection");
        app.Window.InsertText(ref input);
        Assert.Equal(input.Text, name.Text);
        Assert.True(app.Window.RequireKeyboard);
        app.Render();

        // Begin beyond the reserved navigation edge-swipe strip.
        var start = name.LocalToGlobal((name.Frame.X + 35, name.Frame.Center.Y));
        var end = name.LocalToGlobal((name.Frame.X + 85, name.Frame.Center.Y));
        app.PointerDown(start);
        var initialCaret = name.Selection.Start;
        app.PointerMove(end);
        Assert.True(name.Selection.IsEmpty);
        Assert.True(name.Selection.Start > initialCaret);
        Assert.True(name.IsFocused);
        Assert.True(magnifier.Visible);
        Assert.InRange((double)magnifier.Caret.X, (double)name.Frame.Left, (double)name.Frame.Right);
        app.Render();
        app.PointerUp(end);
        Assert.False(magnifier.Visible);

        password.Focus();
        Assert.True(app.Window.IsSecureTextEntry);
        app.Window.InsertText(ref input);
        Assert.True(password.HasText);
        var number = Assert.IsType<TextBox>(app.Window.RootView.FindViewById("NumberBox"));
        var color = Assert.IsType<TextBox>(app.Window.RootView.FindViewById("ColorBox"));
        Assert.Equal(TextInputContentType.Name, name.InputOptions.ContentType);
        Assert.False(name.CanPerformInputAction(TextInputAction.Previous));
        password.PerformInputAction(TextInputAction.Previous);
        Assert.True(name.IsFocused);
        Assert.False(name.CanPerformInputAction(TextInputAction.Previous));
        name.PerformInputAction(TextInputAction.Default);
        Assert.True(password.IsFocused);
        password.PerformInputAction(TextInputAction.Next);
        Assert.True(number.IsFocused);
        Assert.Equal(TextInputKeyboard.Decimal, number.InputOptions.Keyboard);
        Assert.True(number.Editor.Insert("12.75"));
        Assert.False(number.Editor.Insert("a"));
        number.PerformInputAction(TextInputAction.Default);
        Assert.True(color.IsFocused);
        Assert.True(color.Editor.Insert("#aB09ef"));
        Assert.False(color.Editor.Insert("G"));
        color.PerformInputAction(TextInputAction.Next);
        Assert.Null(app.Window.RootView.FocusedView);
        Assert.False(app.Window.RequireKeyboard);
        // Tapping a field opens editing again after completing the form.
        app.PointerDown(rightSide);
        app.PointerUp(rightSide);
        Assert.True(name.IsFocused);
        Assert.True(app.Window.RequireKeyboard);
        // Removing a focused page clears the responder demand as well.
        navigation.Pop();
        app.AnimationFrame(TimeSpan.FromMilliseconds(350), TimeSpan.FromMilliseconds(350));
        app.AnimationFrame(TimeSpan.FromMilliseconds(350), TimeSpan.FromMilliseconds(700));
        app.Render();
        Assert.Null(app.Window.RootView.FocusedView);
        Assert.False(app.Window.RequireKeyboard);
    }

    private sealed class RecordingMagnifier : ITextMagnifier
    {
        public bool IsSupported => true;
        public bool Visible;
        public Rect Caret;
        public void Show(Point position, Rect caret) { Visible = true; Caret = caret; }
        public void Hide() => Visible = false;
    }
}
