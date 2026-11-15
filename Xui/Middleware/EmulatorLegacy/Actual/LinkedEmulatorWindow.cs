using System;
using System.Runtime.InteropServices;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.EmulatorLegacy.Actual;

internal sealed class LinkedEmulatorWindow : Xui.Core.Abstract.IWindow
{
    private Point? leftMouseButtonTouch;

    public Xui.Core.Abstract.IWindow AppWindow { get; }

    // This is an internal event adapter, not a node in the composed-window chain.
    public Xui.Core.Actual.IWindow? Actual => null;

    public LinkedEmulatorWindow(Xui.Core.Abstract.IWindow appWindow)
    {
        AppWindow = appWindow;
    }

    public bool HasActiveTouch => leftMouseButtonTouch.HasValue;

    public Point? ActiveTouchPoint => leftMouseButtonTouch;

    public Rect DisplayArea { get; set; }

    public Rect SafeArea { get; set; }

    public NFloat ScreenCornerRadius { get; set; }

    public void Closed() => AppWindow.Closed();

    public bool Closing() => AppWindow.Closing();

    public object? GetService(Type serviceType) => null;

    public void OnAnimationFrame(ref FrameEventRef animationFrame) =>
        AppWindow.OnAnimationFrame(ref animationFrame);

    public void OnMouseDown(ref MouseDownEventRef evRef)
    {
        AppWindow.OnMouseDown(ref evRef);
        if (evRef.Button != MouseButton.Left)
            return;

        leftMouseButtonTouch = evRef.Position;
        var touchEventRef = new TouchEventRef([new()
        {
            Index = 0,
            Phase = TouchPhase.Start,
            Position = evRef.Position,
            Radius = 0.5f
        }]);
        AppWindow.OnTouch(ref touchEventRef);
    }

    public void OnMouseMove(ref MouseMoveEventRef evRef)
    {
        AppWindow.OnMouseMove(ref evRef);
        if (!leftMouseButtonTouch.HasValue)
            return;

        leftMouseButtonTouch = evRef.Position;
        var touchEventRef = new TouchEventRef([new()
        {
            Index = 0,
            Phase = TouchPhase.Move,
            Position = evRef.Position,
            Radius = 0.5f
        }]);
        AppWindow.OnTouch(ref touchEventRef);
    }

    public void OnMouseUp(ref MouseUpEventRef evRef)
    {
        AppWindow.OnMouseUp(ref evRef);
        if (evRef.Button != MouseButton.Left)
            return;

        if (!leftMouseButtonTouch.HasValue)
            return;

        leftMouseButtonTouch = null;
        var touchEventRef = new TouchEventRef([new()
        {
            Index = 0,
            Phase = TouchPhase.End,
            Position = evRef.Position,
            Radius = 0.5f
        }]);
        AppWindow.OnTouch(ref touchEventRef);
    }

    public void OnScrollWheel(ref ScrollWheelEventRef evRef) =>
        AppWindow.OnScrollWheel(ref evRef);

    public void OnTouch(ref TouchEventRef touchEventRef) =>
        AppWindow.OnTouch(ref touchEventRef);

    public void Render(ref RenderEventRef render)
    {
        AppWindow.DisplayArea = DisplayArea;
        AppWindow.SafeArea = SafeArea;
        AppWindow.ScreenCornerRadius = ScreenCornerRadius;
        AppWindow.Render(ref render);
    }

    public void WindowHitTest(ref WindowHitTestEventRef evRef) =>
        AppWindow.WindowHitTest(ref evRef);

    public void OnKeyDown(ref KeyEventRef e) =>
        AppWindow.OnKeyDown(ref e);

    public void OnChar(ref KeyEventRef e) =>
        AppWindow.OnChar(ref e);

    public void RenderTouchIndicator(IContext ctx)
    {
        if (!leftMouseButtonTouch.HasValue)
            return;

        ctx.BeginPath();
        ctx.Ellipse(leftMouseButtonTouch.Value, 15f, 15f, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
        ctx.SetFill(0x66888888);
        ctx.Fill();

        ctx.BeginPath();
        ctx.Ellipse(leftMouseButtonTouch.Value, 15f, 15f, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
        ctx.LineWidth = 3f;
        ctx.SetStroke(0x88AAAAAA);
        ctx.Stroke();
    }
}
