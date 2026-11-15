using System.Runtime.CompilerServices;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Examples;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class TextBoxTest
{
    private static void Scenario(TestRuntimeVariant variant, Action<TestSinglePageApp<Application, MainWindow>> scenario,
        [CallerFilePath] string path = "", [CallerMemberName] string name = "")
    {
        using var app = IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((600, 400), variant, path, name);
        app.Run(() =>
        {
            app.Prepare();
            app.Open<TextBoxExample>("TextBox");
            app.MarkdownHeading(name, 1);
            scenario(app);
        });
    }

    private static TextBox Field(TestSinglePageApp<Application, MainWindow> app, string id)
        => app.Target<TextBox>(id, "editing Text Input");

    private static TextBox Focus(TestSinglePageApp<Application, MainWindow> app, string id)
    {
        app.Tap(id);
        var field = Field(app, id);
        Assert.True(field.IsFocused, $"Expected '{id}' to receive focus after tapping it.");
        return field;
    }

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_Focus(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        app.Snapshot("TextBoxPage");
        Focus(app, "NameBox");
        app.Snapshot("NameBoxFocused");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_Type(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        var name = Focus(app, "NameBox");
        app.Type("Hello"); Assert.Equal("Hello", name.Text); app.Snapshot("Typed.Hello");
        app.Type(" World"); Assert.Equal("Hello World", name.Text); app.Snapshot("Typed.HelloWorld");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_Backspace(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        var name = Focus(app, "NameBox");
        app.Type("Hello"); Assert.Equal("Hello", name.Text); app.Snapshot("BeforeBackspace");
        app.KeyDown(VirtualKey.Back); app.KeyDown(VirtualKey.Back);
        Assert.Equal("Hel", name.Text); app.Snapshot("AfterBackspace");
        app.Type("p"); Assert.Equal("Help", name.Text); app.Snapshot("AfterRetype");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_Password(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        var password = Focus(app, "PasswordBox");
        app.Type("secret"); Assert.Equal("secret", password.Text);
        var svg = app.Snapshot("PasswordTyped");
        Assert.DoesNotContain(">secret<", svg); Assert.Contains("••••••", svg);
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_SwitchFocus(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        var name = Focus(app, "NameBox"); app.Type("Alice"); app.Snapshot("NameFilled");
        var password = Focus(app, "PasswordBox"); Assert.False(name.IsFocused); app.Snapshot("PasswordFocused");
        app.Type("pass"); Assert.Equal("Alice", name.Text); Assert.Equal("pass", password.Text);
        app.Snapshot("PasswordFilled");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_TabNavigation(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        Focus(app, "NameBox");
        app.Type("Tab1"); Assert.Equal("Tab1", Field(app, "NameBox").Text); app.Snapshot("Tab.NameBox");
        foreach (var id in new[] { "PasswordBox", "NumberBox", "ColorBox" })
        {
            app.KeyDown(VirtualKey.Tab);
            Assert.True(Field(app, id).IsFocused, $"Tab should focus '{id}'.");
            if (id == "PasswordBox") { app.Type("pass"); Assert.Equal("pass", Field(app, id).Text); }
            app.Snapshot("Tab." + id);
        }
        app.KeyDown(VirtualKey.Tab);
        Assert.True(Field(app, "NameBox").IsFocused); app.Snapshot("Tab.WrapToNameBox");
        app.KeyDown(VirtualKey.Tab, shift: true);
        Assert.True(Field(app, "ColorBox").IsFocused); app.Snapshot("ShiftTab.ColorBox");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_KeyboardSelection(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        var name = Focus(app, "NameBox");
        app.Type("Hello World!"); Assert.Equal("Hello World!", name.Text); app.Snapshot("Typed");
        app.KeyDown(VirtualKey.Left); app.KeyDown(VirtualKey.Left);
        Assert.Equal((uint)10, name.Selection.Start); app.Snapshot("AfterLeftLeft");
        app.KeyDown(VirtualKey.Left, shift: true); app.KeyDown(VirtualKey.Left, shift: true);
        Assert.Equal((uint)8, name.Selection.Start); Assert.Equal((uint)10, name.Selection.End); app.Snapshot("ShiftSelected");
        app.Type("yep"); Assert.Equal("Hello Woyepd!", name.Text); app.Snapshot("AfterReplace");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextBox_MouseSelection(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        var name = Focus(app, "NameBox"); app.Type("Hello World!"); app.Render();
        // Physical mouse selection remains useful on mobile, independently of touch loupe gestures.
        // Use measured caret positions rather than character-width or screen-coordinate guesses.
        var measure = app.Window.TextMeasureContext!;
        var start = name.LocalToGlobal(name.GetCaretRect(2, measure).Center);
        var end = name.LocalToGlobal(name.GetCaretRect(7, measure).Center);
        var down = new MouseDownEventRef { Position = start, Button = MouseButton.Left };
        app.Window.OnMouseDown(ref down); app.Snapshot("MouseDown");
        var move = new MouseMoveEventRef { Position = end };
        app.Window.OnMouseMove(ref move); app.Snapshot("MouseDrag");
        var up = new MouseUpEventRef { Position = end, Button = MouseButton.Left };
        app.Window.OnMouseUp(ref up);
        Assert.False(name.Selection.IsEmpty);
        Assert.Equal((uint)2, name.Selection.Start); Assert.Equal((uint)7, name.Selection.End);
        app.Snapshot("MouseUp");
    });
}
