using System;
using System.Collections.Generic;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Middleware.Emulator.Devices;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.UI.Phone;

/// <summary>Hosts an emulated display inside its phone chrome and bridges its rendering and touch input.</summary>
internal sealed class PhoneFrameView(DeviceProfile device) : View
{
    private readonly PhoneFrameSpecification specification = PhoneFrameSpecification.IPhone;
    private readonly PhoneChromeRenderer chromeRenderer = new();
    private readonly TouchOverlay touchOverlay = new();
    private readonly HashSet<int> activeTouchPointers = [];
    private EmulatedWindow? emulatedWindow;
    private PhoneFrameGeometry geometry;
    private Point lastPointerPosition;
    private bool hasPointerPosition;

    public Rect OuterFrame => geometry.OuterFrame;
    public Rect ScreenFrame => geometry.ScreenFrame;
    public NFloat Scale => geometry.Scale;

    internal void Attach(EmulatedWindow emulatedWindow) => this.emulatedWindow = emulatedWindow;

    public override bool HitTest(Point point) => ScreenFrame.Contains(point);

    protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
    {
        var inset = specification.TotalInset;
        return new Size(
            device.LogicalResolution.Width + inset * 2,
            device.LogicalResolution.Height + inset * 2);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context) =>
        geometry = PhoneFrameGeometry.Create(rect, device, specification);

    protected override void RenderCore(IContext context)
    {
        chromeRenderer.RenderFrame(context, device, specification, in geometry);
        RenderApp(context);
        RenderTouchOverlay(context);
        chromeRenderer.RenderDisplayCutout(context, device, in geometry);
    }

    private void RenderApp(IContext context)
    {
        if (emulatedWindow is null)
            return;

        context.Save();
        try
        {
            context.BeginPath();
            context.RoundRect(ScreenFrame, device.ScreenCornerRadius * Scale);
            context.Clip();
            context.Translate(ScreenFrame.TopLeft);
            context.Scale((Scale, Scale));

            var displayArea = new Rect(0, 0, device.LogicalResolution.Width, device.LogicalResolution.Height);
            var render = new RenderEventRef(displayArea, default, context);
            emulatedWindow.Render(ref render);
        }
        finally
        {
            context.Restore();
        }
    }

    private void RenderTouchOverlay(IContext context)
    {
        context.Save();
        try
        {
            context.BeginPath();
            context.RoundRect(ScreenFrame, device.ScreenCornerRadius * Scale);
            context.Clip();
            touchOverlay.Render(context, Scale);
        }
        finally
        {
            context.Restore();
        }
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble || Scale <= 0)
            return;

        // Wheel events do not carry a position. Remember the latest desktop
        // pointer position over the display so a host touchpad gesture can be
        // hit-tested against the corresponding emulated view.
        if (e.State.PointerType == PointerType.Mouse)
        {
            lastPointerPosition = geometry.MapHostToDevice(e.State.Position);
            hasPointerPosition = true;
        }

        if (emulatedWindow is null)
            return;

        if (e.Type == PointerEventType.Down)
        {
            activeTouchPointers.Add(e.PointerId);
            if (e.State.PointerType == PointerType.Mouse)
                touchOverlay.Begin(e.PointerId, e.State.Position);
            CapturePointer(e.PointerId);
            DispatchTouch(ref e, TouchPhase.Start);
            InvalidateRender();
            return;
        }

        if (!activeTouchPointers.Contains(e.PointerId))
            return;

        if (e.Type == PointerEventType.Move)
        {
            if (e.State.PointerType == PointerType.Mouse)
                touchOverlay.Move(e.PointerId, e.State.Position);
            DispatchTouch(ref e, TouchPhase.Move);
            InvalidateRender();
            return;
        }

        if (e.Type is PointerEventType.Up or PointerEventType.Cancel)
        {
            if (e.State.PointerType == PointerType.Mouse)
                touchOverlay.End(e.PointerId);
            DispatchTouch(ref e, TouchPhase.End);
            activeTouchPointers.Remove(e.PointerId);
            ReleasePointer(e.PointerId);
            InvalidateRender();
        }
    }

    /// <summary>
    /// Forwards host wheel and touchpad input through the emulator chrome into
    /// the mobile view tree. This is intentionally an emulator-only convenience:
    /// physical mobile platforms continue to use touch input.
    /// </summary>
    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        if (e.Handled || emulatedWindow is null || !hasPointerPosition)
            return;

        emulatedWindow.DispatchScrollWheel(ref e, lastPointerPosition);
    }

    private void DispatchTouch(ref PointerEventRef e, TouchPhase phase)
    {
        var position = geometry.MapHostToDevice(e.State.Position);
        var radius = NFloat.Max(e.State.ContactSize.Width, e.State.ContactSize.Height) / (2 * Scale);
        Span<Touch> touches = stackalloc Touch[1];
        touches[0] = new Touch
        {
            Index = e.PointerId,
            Position = position,
            Radius = radius,
            Phase = phase,
        };
        var touch = new TouchEventRef(touches);
        emulatedWindow!.DispatchTouch(ref touch);
    }
}
