using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.Middleware.EmulatorLegacy.UI;

/// <summary>Visual-only disclosure button for the device selector.</summary>
internal sealed class EmulatorHeaderChevronButtonView : View
{
    private bool hover;
    private bool pressed;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType != PointerType.Mouse)
            return;

        if (e.Type == PointerEventType.Enter)
            hover = true;
        else if (e.Type == PointerEventType.Leave)
        {
            hover = false;
            pressed = false;
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down && e.State.Button == PointerButton.Left)
            pressed = true;
        else if (phase == EventPhase.Tunnel && e.Type is PointerEventType.Up or PointerEventType.Cancel)
            pressed = false;
        else
            return;

        InvalidateRender();
    }

    protected override void RenderCore(IContext context)
    {
        var center = Frame.Center;
        if (hover || pressed)
        {
            context.BeginPath();
            var radius = pressed ? 14f : 15f;
            context.Ellipse(center, radius, radius, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
            context.SetFill(pressed ? new Color(0xDFDFDFFF) : new Color(0xF2F2F2FF));
            context.Fill();
        }

        context.BeginPath();
        context.MoveTo(center + (-4f, -1.5f));
        context.LineTo(center + (0, 2.5f));
        context.LineTo(center + (4f, -1.5f));
        context.SetStroke(new Color(0x4A4A4AFF));
        context.LineWidth = 1.5f;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        context.Stroke();
    }
}
