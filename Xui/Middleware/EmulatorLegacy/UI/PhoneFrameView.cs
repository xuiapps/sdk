using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Middleware.EmulatorLegacy.Actual;

namespace Xui.Middleware.EmulatorLegacy.UI;

/// <summary>Draws the device frame and translates desktop pointer input into mobile input.</summary>
internal sealed class PhoneFrameView : View
{
    private readonly EmulatorWindow emulator;
    private readonly EmulatorRootView emulatorRoot;
    private readonly EmulatorChromeRenderer chromeRenderer = new();
    private EmulatorGeometry geometry;

    public PhoneFrameView(EmulatorWindow emulator)
    {
        this.emulator = emulator;
        this.emulatorRoot = new EmulatorRootView(emulator);
        this.SetProtectedChild(ref this.child, emulatorRoot);
    }

    private View? child;

    public override int Count => child is null ? 0 : 1;

    public override View this[int index] =>
        index == 0 && child is not null ? child : throw new IndexOutOfRangeException();

    public EmulatorGeometry Geometry => geometry;

    /// <summary>Maps a logical mobile point into the host window.</summary>
    public Point MapEmulatorToHost(Point point) => geometry.MapEmulatorToHost(point);

    /// <summary>Maps a host-window point into logical mobile coordinates.</summary>
    public Point MapHostToEmulator(Point point) => geometry.MapHostToEmulator(point);

    /// <summary>Maps a logical mobile rectangle into the host window.</summary>
    public Rect MapEmulatorToHost(Rect rect) => new(
        MapEmulatorToHost(rect.TopLeft).X,
        MapEmulatorToHost(rect.TopLeft).Y,
        rect.Width * geometry.Scale,
        rect.Height * geometry.Scale);

    /// <summary>Maps a host-window rectangle into logical mobile coordinates.</summary>
    public Rect MapHostToEmulator(Rect rect) => new(
        MapHostToEmulator(rect.TopLeft).X,
        MapHostToEmulator(rect.TopLeft).Y,
        rect.Width / geometry.Scale,
        rect.Height / geometry.Scale);

    public override bool HitTest(Point point) =>
        emulatorRoot.HitTest(point);

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        geometry = EmulatorGeometry.Create(rect, emulator.CurrentDevice);
        emulatorRoot.Arrange(geometry.EmulatorRect, context);
    }

    protected override void RenderCore(IContext context)
    {
        emulatorRoot.Render(context);
        var frame = this.Frame;
        chromeRenderer.Render(
            context,
            in geometry,
            in frame,
            emulator.CurrentDevice,
            emulator.ResolveStatusBarStyle(),
            emulator.ResolveClock());
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble)
            return;

        if (e.Type == PointerEventType.Down)
            this.CapturePointer(e.PointerId);

        emulatorRoot.DispatchPointer(ref e, in geometry);

        if (e.Type is PointerEventType.Up or PointerEventType.Cancel)
            this.ReleasePointer(e.PointerId);
    }
}
