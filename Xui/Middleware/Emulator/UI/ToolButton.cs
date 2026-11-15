using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.UI;

/// <summary>An icon-only toolbar button with macOS-style hover and pressed feedback.</summary>
internal sealed class ToolButton(IToolButtonIcon icon) : View
{
    private bool hover;
    private bool pressed;

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
        if (hover || pressed)
        {
            context.BeginPath();
            var radius = pressed ? 14f : 15f;
            context.Ellipse(Frame.Center, radius, radius, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
            context.SetFill(pressed ? new Color(0xDFDFDFFF) : new Color(0xF2F2F2FF));
            context.Fill();
        }

        icon.Render(context, Frame.Center);
    }
}
