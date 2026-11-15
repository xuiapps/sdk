using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.Apps.TestApp.Pages;

/// <summary>A selectable row in a mobile example section sheet.</summary>
internal sealed class MobileExampleSectionMenuItem(Func<string> text, Action select) : View
{
    private bool pressed;

    protected override Size MeasureCore(Size available, IMeasureContext context) => new(available.Width, 44);

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            pressed = true;
            CapturePointer(e.PointerId, PointerGestures.Tap);
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type is PointerEventType.Up or PointerEventType.Cancel)
        {
            var wasPressed = pressed;
            pressed = false;
            ReleasePointer(e.PointerId);
            if (wasPressed && e.Type == PointerEventType.Up)
                select();
            InvalidateRender();
        }
    }

    protected override void RenderCore(IContext context)
    {
        if (pressed)
        {
            context.SetFill(new Color(0xE8, 0xE8, 0xEC, 0xFF));
            context.FillRect(this.Frame);
        }
        context.SetFont(new Font(16, "Inter"));
        context.TextAlign = TextAlign.Left;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(Colors.Black);
        context.FillText(text(), new Point(this.Frame.X + 20, this.Frame.Center.Y));
    }
}
