using System;
using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Middleware.EmulatorLegacy.Actual;

namespace Xui.Middleware.EmulatorLegacy.UI;

/// <summary>Virtual mobile render surface hosted inside a <see cref="PhoneFrameView"/>.</summary>
internal sealed class EmulatorRootView(EmulatorWindow emulator) : View
{
    public override int Count => 0;

    public override View this[int index] => throw new IndexOutOfRangeException();

    protected override void RenderCore(IContext context)
    {
        var geometry = emulator.Geometry;
        RenderContent(context, in geometry, emulator.CurrentDevice, emulator.RenderTouchIndicators);
    }

    public void RenderSnapshot(IContext context, in EmulatorGeometry geometry) =>
        RenderContent(context, in geometry, emulator.CurrentDevice, emulator.RenderTouchIndicators);

    private void RenderContent(IContext context, in EmulatorGeometry geometry, Devices.DeviceProfile device, bool renderTouchIndicators)
    {
        context.Save();
        try
        {
            context.BeginPath();
            context.RoundRect(geometry.EmulatorRect, geometry.ScreenCornerRadius);
            context.Clip();
            context.Translate(geometry.EmulatorRect.TopLeft);
            if (geometry.Scale != 1)
                context.Scale((geometry.Scale, geometry.Scale));

            var displayArea = new Rect(0, 0, device.LogicalResolution.Width, device.LogicalResolution.Height);
            var render = new RenderEventRef(displayArea, default, context);
            if (emulator.AppWindow is not Xui.Core.Abstract.Window
                { RootView.Content: IFullScreenView { DrawsOpaqueFullScreen: true } })
            {
                context.SetFill(Colors.White);
                context.FillRect(displayArea);
            }

            emulator.SetMobileDisplay(displayArea, device);
            emulator.RenderMobile(ref render);
            if (renderTouchIndicators)
                emulator.RenderTouchIndicator(context);
        }
        finally
        {
            context.Restore();
        }
    }

    public void DispatchPointer(ref PointerEventRef e, in EmulatorGeometry geometry)
    {
        if (e.Type is not (PointerEventType.Down or PointerEventType.Move or PointerEventType.Up or PointerEventType.Cancel))
            return;

        var position = geometry.MapHostToEmulator(e.State.Position);
        if (e.State.PointerType == PointerType.Touch)
        {
            var touch = new TouchEventRef([new()
            {
                Index = e.PointerId,
                Phase = e.Type switch
                {
                    PointerEventType.Down => TouchPhase.Start,
                    PointerEventType.Move => TouchPhase.Move,
                    _ => TouchPhase.End
                },
                Position = position,
                Radius = NFloat.Max(e.State.ContactSize.Width, e.State.ContactSize.Height) / (2 * geometry.Scale)
            }]);
            emulator.OnTouch(ref touch);
            return;
        }

        switch (e.Type)
        {
            case PointerEventType.Down:
            {
                var mouse = new MouseDownEventRef { Position = position, Button = MouseButton.Left, TextMeasure = e.TextMeasure };
                emulator.OnMouseDown(ref mouse);
                break;
            }
            case PointerEventType.Move:
            {
                var mouse = new MouseMoveEventRef { Position = position, TextMeasure = e.TextMeasure };
                emulator.OnMouseMove(ref mouse);
                break;
            }
            default:
            {
                var mouse = new MouseUpEventRef { Position = position, Button = MouseButton.Left, TextMeasure = e.TextMeasure };
                emulator.OnMouseUp(ref mouse);
                break;
            }
        }
    }
}
