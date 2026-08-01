using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;
using Window = Xui.Core.Abstract.Window;

namespace Xui.Core.UI.Tests;

public class TextSelectionTests
{
    [Fact]
    public void KeyboardActionsMoveWithoutWrappingAndCanFocusANonTextControl()
    {
        using var fixture = new Fixture();
        var second = new TextBox { InputOptions = new(ReturnAction: TextInputAction.Next) };
        var submit = new FocusTarget();
        fixture.Host.Content = null;
        fixture.Window.RootView.Content = new VerticalStack { Content = [fixture.Box, second, submit] };
        Assert.Same(fixture.Box, ((IFocus)fixture.Window.RootView).GetNext());
        Assert.Same(submit, ((IFocus)fixture.Window.RootView).GetPrevious());
        fixture.Box.InputOptions = new(ReturnAction: TextInputAction.Next);
        fixture.Box.Focus();
        Assert.False(fixture.Box.CanPerformInputAction(TextInputAction.Previous));
        var enter = new KeyEventRef { Key = VirtualKey.Return };
        fixture.Window.OnKeyDown(ref enter);
        Assert.True(second.IsFocused);
        second.PerformInputAction(TextInputAction.Previous);
        Assert.True(fixture.Box.IsFocused);
        var newline = new InsertTextEventRef("\n");
        fixture.Window.InsertText(ref newline);
        Assert.True(second.IsFocused);
        second.PerformInputAction(TextInputAction.Next);
        Assert.True(submit.IsFocused);
        Assert.False(fixture.Window.RequireKeyboard);
        second.InputOptions = new(ReturnAction: TextInputAction.Done);
        second.Focus();
        second.PerformInputAction(TextInputAction.Next);
        Assert.Null(fixture.Window.RootView.FocusedView);
        Assert.False(fixture.Window.RequireKeyboard);
        // Hardware Tab retains its existing wrapping behavior.
        submit.Focus();
        ((IFocus)fixture.Window.RootView).Next();
        Assert.True(fixture.Box.IsFocused);
    }

    private sealed class FocusTarget : View { public override bool Focusable => true; }

    [Fact]
    public void TouchMovesCaretAndMapsLoupeThroughPresentationTransform()
    {
        using var fixture = new Fixture();
        fixture.Host.Transform = new AffineTransform(0.8f, 0, 0, 0.8f, 40, 80);
        fixture.Touch(TouchPhase.Start, 1);
        Assert.True(fixture.Window.RequireKeyboard);
        Assert.Equal((uint)0, fixture.Box.Selection.Start);
        Assert.Equal((uint)6, fixture.Box.Selection.End);
        Assert.Equal(0, fixture.Magnifier.Shows); // A tap alone does not flash a loupe.
        fixture.Touch(TouchPhase.Move, 4);
        Assert.True(fixture.Box.Selection.IsEmpty);
        Assert.Equal((uint)4, fixture.Box.Selection.Start);
        Assert.Equal(1, fixture.Magnifier.Shows);
        Assert.InRange((double)(fixture.Magnifier.Position.X - fixture.PointAt(4).X), -0.001, 0.001);
        Assert.InRange((double)(fixture.Magnifier.Position.Y - fixture.PointAt(4).Y), -0.001, 0.001);
        Assert.InRange((double)(fixture.Magnifier.Caret.X - fixture.PointAt(4).X), -0.01, 0.01);
        Assert.InRange((double)fixture.Magnifier.Caret.Width, 0.799, 0.801);
        fixture.Touch(TouchPhase.End, 4);
        Assert.Equal(1, fixture.Magnifier.Hides);
        Assert.Null(fixture.Window.RootView.EventRouter.GetCapturedView(0));
        Assert.True(fixture.Window.RequireKeyboard); // Release keeps editing focus.
    }

    [Theory]
    [InlineData("cancel")]
    [InlineData("capture")]
    [InlineData("blur")]
    [InlineData("remove")]
    [InlineData("close")]
    public void GestureAndFocusLifecycleDismissesLoupe(string reason)
    {
        using var fixture = new Fixture();
        fixture.Touch(TouchPhase.Start, 1);
        fixture.Touch(TouchPhase.Move, 4);
        switch (reason)
        {
            case "cancel": fixture.Touch(TouchPhase.Cancel, 4); break;
            case "capture": fixture.Window.RootView.EventRouter.CapturePointer(fixture.Host, 0); break;
            case "blur": fixture.Box.Blur(); break;
            case "remove": fixture.Host.Content = null; break;
            case "close": fixture.Window.Dispose(); break;
        }
        Assert.Equal(1, fixture.Magnifier.Hides);
        Assert.NotSame(fixture.Box, fixture.Window.RootView.EventRouter.GetCapturedView(0));
        if (reason is "blur" or "remove" or "close") Assert.False(fixture.Window.RequireKeyboard);
    }

    [Fact]
    public void OtherFingersDoNotMoveOrDismissActiveSelection()
    {
        using var fixture = new Fixture();
        fixture.Touch(TouchPhase.Start, 1);
        fixture.Touch(TouchPhase.Move, 4);
        fixture.Touch(TouchPhase.Start, 2, 1);
        fixture.Touch(TouchPhase.Move, 5, 1);
        fixture.Touch(TouchPhase.End, 5, 1);
        Assert.Equal((uint)4, fixture.Box.Selection.Start);
        Assert.Equal(1, fixture.Magnifier.Shows);
        Assert.Equal(0, fixture.Magnifier.Hides);
        fixture.Touch(TouchPhase.End, 4);
        Assert.Equal(1, fixture.Magnifier.Hides);
    }

    [Fact]
    public void MouseDragStillSelectsARangeWithoutLoupe()
    {
        using var fixture = new Fixture();
        var down = new MouseDownEventRef { Position = fixture.PointAt(1) };
        fixture.Window.OnMouseDown(ref down);
        var move = new MouseMoveEventRef { Position = fixture.PointAt(4) };
        fixture.Window.OnMouseMove(ref move);
        var up = new MouseUpEventRef { Position = fixture.PointAt(4) };
        fixture.Window.OnMouseUp(ref up);
        Assert.Equal((uint)1, fixture.Box.Selection.Start);
        Assert.Equal((uint)4, fixture.Box.Selection.End);
        Assert.Equal(0, fixture.Magnifier.Shows);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrUnsupportedLoupeStillAllowsCaretDragging(bool absent)
    {
        using var fixture = new Fixture();
        fixture.Runtime.MagnifierAvailable = !absent;
        fixture.Magnifier.IsSupported = false;
        fixture.Touch(TouchPhase.Start, 1);
        fixture.Touch(TouchPhase.Move, 4);
        fixture.Touch(TouchPhase.End, 4);
        Assert.Equal((uint)4, fixture.Box.Selection.Start);
        Assert.True(fixture.Box.Selection.IsEmpty);
        Assert.Equal(0, fixture.Magnifier.Shows);
    }

    [Fact]
    public void NativeKeyboardInsertionAndDeletionReachFocusedInput()
    {
        using var fixture = new Fixture();
        fixture.Box.Focus(); // Select all on focus.
        var insert = new InsertTextEventRef("Hello");
        fixture.Window.InsertText(ref insert);
        Assert.Equal("Hello", fixture.Box.Text);
        Assert.True(fixture.Window.HasText);
        fixture.Box.IsPassword = true;
        Assert.True(fixture.Window.IsSecureTextEntry);
        var delete = new DeleteBackwardsEventRef();
        fixture.Window.DeleteBackwards(ref delete);
        Assert.Equal("Hell", fixture.Box.Text);
        fixture.Box.Blur();
        fixture.Window.InsertText(ref insert);
        Assert.Equal("Hell", fixture.Box.Text);
        Assert.False(fixture.Window.RequireKeyboard);
        Assert.False(fixture.Window.HasText);
        Assert.False(fixture.Window.IsSecureTextEntry);
    }

    [Fact]
    public void FirstTouchSelectsAllAndSecondTouchPlacesCaret()
    {
        using var fixture = new Fixture();
        fixture.Touch(TouchPhase.Start, 2);
        fixture.Touch(TouchPhase.End, 2);
        Assert.Equal(new TextRange(0, 6), fixture.Box.Editor.Selection);
        fixture.Touch(TouchPhase.Start, 4);
        fixture.Touch(TouchPhase.End, 4);
        Assert.Equal(new TextRange(4, 4), fixture.Box.Editor.Selection);
    }

    [Fact]
    public void NativeSessionSharesGeometryAndEditsAndEndsOnRemoval()
    {
        using var fixture = new Fixture();
        fixture.Runtime.SessionAvailable = true;
        fixture.Host.Transform = new AffineTransform(0.8f, 0, 0, 0.8f, 40, 80);
        fixture.Touch(TouchPhase.Start, 1);
        var session = fixture.Runtime.Session;
        Assert.Same(fixture.Box, session.Client);
        Assert.True(fixture.Box.NativeSelection);
        var caret = session.Client!.GetCaretRect(4, fixture.Runtime.Measure);
        Assert.InRange((double)(caret.X - fixture.PointAt(4).X), -0.01, 0.01);
        Assert.Equal(4, session.Client.GetTextPosition(fixture.PointAt(4), fixture.Runtime.Measure));
        fixture.Touch(TouchPhase.Move, 4);
        Assert.Equal(new TextRange(0, 6), fixture.Box.Editor.Selection); // UIKit owns the drag.
        Assert.Equal(0, fixture.Magnifier.Shows);
        session.Client.Editor.Selection = new(2, 4);
        var input = new InsertTextEventRef("👨‍👩‍👧‍👦");
        fixture.Window.InsertText(ref input);
        Assert.Equal("ab👨‍👩‍👧‍👦ef", fixture.Box.Text);
        fixture.Host.Content = null;
        Assert.Null(session.Client);
        Assert.False(fixture.Box.NativeSelection);
        Assert.False(fixture.Window.RequireKeyboard);
    }

    private sealed class Session : ITextInputSession
    {
        public ITextEditingClient? Client;
        public void SetClient(ITextEditingClient? client)
        {
            if (Client is not null) Client.NativeSelection = false;
            Client = client;
            if (Client is not null) Client.NativeSelection = true;
        }
        public void CancelSelectionGesture() { }
    }

    private sealed class Fixture : IDisposable
    {
        public readonly FakeRuntime Runtime = new();
        public readonly Window Window;
        public readonly TextBox Box = new() { Text = "abcdef" };
        public readonly TransformView Host;
        public Magnifier Magnifier => Runtime.Magnifier;
        public Fixture()
        {
            Window = new Window(Runtime);
            Host = new TransformView { Content = Box };
            Window.RootView.Content = Host;
            Window.RootView.Arrange((0, 0, 600, 400), Runtime.Measure);
        }
        // TextBox's one-point border plus three-point padding, with 10pt test glyph advances.
        public Point PointAt(int index) => Box.LocalToGlobal((Box.Frame.X + 4 + index * 10, Box.Frame.Y + 20));
        public void Touch(TouchPhase phase, int index, int pointer = 0)
        {
            Span<Touch> touches = stackalloc Touch[1];
            touches[0] = new Touch { Index = pointer, Phase = phase, Position = PointAt(index) };
            var e = new TouchEventRef(touches);
            Window.OnTouch(ref e);
        }
        public void Dispose() => Window.Dispose();
    }

    private sealed class Magnifier : ITextMagnifier
    {
        public bool IsSupported { get; set; } = true;
        public int Shows, Hides;
        public Point Position;
        public Rect Caret;
        public void Show(Point position, Rect caret) { Shows++; Position = position; Caret = caret; }
        public void Hide() => Hides++;
    }

    private sealed class MeasureContext : IMeasureContext
    {
        public void SetFont(Font font) { }
        public TextMetrics MeasureText(string text) => MeasureText(text.AsSpan());
        public TextMetrics MeasureText(ReadOnlySpan<char> text) => new(
            new LineMetrics(text.Length * 10, 0, text.Length * 10, 15, 5),
            new FontMetrics(15, 5, 15, 5, 0, 0, 0));
    }

    private sealed class FakeRuntime : IRuntime, IServiceProvider
    {
        public readonly Magnifier Magnifier = new();
        public readonly MeasureContext Measure = new();
        public bool MagnifierAvailable = true;
        public bool SessionAvailable;
        public readonly Session Session = new();
        public IDispatcher MainDispatcher => throw new NotSupportedException();
        public IRunLoop CreateRunloop(Abstract.Application application) => throw new NotSupportedException();
        public IWindow CreateWindow(Abstract.IWindow window, IServiceProvider services) => new FakeWindow(window, this);
        public object? GetService(Type type) => type == typeof(IRuntime) ? this
            : type == typeof(ITextMagnifier) && MagnifierAvailable ? Magnifier
            : type == typeof(ITextInputSession) && SessionAvailable ? Session : null;
        private sealed class FakeWindow(Abstract.IWindow window, FakeRuntime runtime) : IWindow
        {
            public Abstract.IWindow Abstract => window;
            public IServiceProvider NextServiceProvider => runtime;
            public string Title { get; set; } = "";
            public bool RequireKeyboard { get; set; }
            public ITextMeasureContext TextMeasureContext => runtime.Measure;
            public void Show() { }
            public void Invalidate() { }
            public object? GetService(Type type) => runtime.GetService(type);
        }
    }
}
