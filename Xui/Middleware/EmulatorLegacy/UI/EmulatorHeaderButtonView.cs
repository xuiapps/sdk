using System;
using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.Middleware.EmulatorLegacy.UI;

internal sealed class EmulatorHeaderButtonView : View
{
    private readonly bool clockwise;
    private bool hover;
    private bool pressed;

    public EmulatorHeaderButtonView(bool clockwise)
    {
        this.clockwise = clockwise;
    }

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType != PointerType.Mouse)
            return;

        if (e.Type == PointerEventType.Enter)
        {
            hover = true;
            InvalidateRender();
        }
        else if (e.Type == PointerEventType.Leave)
        {
            hover = false;
            pressed = false;
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down && e.State.Button == PointerButton.Left)
        {
            pressed = true;
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type is PointerEventType.Up or PointerEventType.Cancel)
        {
            pressed = false;
            InvalidateRender();
        }
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

        context.Save();
        try
        {
            context.Translate(center);
            if (!clockwise)
                context.Scale((-1, 1));

            context.BeginPath();
            context.Arc((0, 0), 6.5f, -NFloat.Pi * .18f, NFloat.Pi * 1.4f, Winding.ClockWise);
            context.SetStroke(new Color(0x4A4A4AFF));
            context.LineWidth = 1.5f;
            context.LineCap = LineCap.Round;
            context.LineJoin = LineJoin.Round;
            context.Stroke();

            var tipAngle = NFloat.Pi * 1.4f;
            var tip = new Point(NFloat.Cos(tipAngle) * 6.5f, NFloat.Sin(tipAngle) * 6.5f);
            context.BeginPath();
            context.MoveTo(tip);
            context.LineTo(tip + (4f, -1f));
            context.LineTo(tip + (1f, 3.5f));
            context.ClosePath();
            context.SetFill(new Color(0x4A4A4AFF));
            context.Fill();
        }
        finally
        {
            context.Restore();
        }
    }
}
