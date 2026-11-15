using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>A compact graphical cog that opens the mobile TestApp settings.</summary>
internal sealed class MobileSettingsButton : View
{
    private bool pressed;

    protected override Size MeasureCore(Size available, IMeasureContext context) => (48, 48);

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Tunnel)
            return;
        if (e.Type == PointerEventType.Down)
        {
            pressed = true;
            CapturePointer(e.PointerId, PointerGestures.Tap);
            InvalidateRender();
        }
        else if (e.Type is PointerEventType.Up or PointerEventType.Cancel)
        {
            var activate = pressed && e.Type == PointerEventType.Up && Frame.Contains(e.State.Position);
            pressed = false;
            ReleasePointer(e.PointerId);
            InvalidateRender();
            if (activate && this.TryFindParent<MobileNavigation>(out var navigation))
                navigation.Push(new MobileSettingsPage());
        }
    }

    protected override void RenderCore(IContext context)
    {
        var frame = Frame.Expand(-3);
        context.BeginPath();
        context.RoundRect(frame, frame.Height / 2);
        context.SetFill(pressed ? new Color(0xD9DCE3FF) : new Color(0xEEF0F4FF));
        context.Fill();

        var center = Frame.Center;
        context.SetStroke(new Color(0x242529FF));
        context.LineWidth = 2.5f;
        context.LineCap = LineCap.Round;
        for (var index = 0; index < 8; index++)
        {
            var angle = index * Math.PI / 4;
            var direction = new Vector((NFloat)Math.Cos(angle), (NFloat)Math.Sin(angle));
            context.BeginPath();
            context.MoveTo(center + direction * 8);
            context.LineTo(center + direction * 11);
            context.Stroke();
        }
        context.BeginPath();
        context.Arc(center, 7, 0, (NFloat)(Math.PI * 2));
        context.Stroke();
        context.BeginPath();
        context.Arc(center, 2.5f, 0, (NFloat)(Math.PI * 2));
        context.SetFill(new Color(0x242529FF));
        context.Fill();
    }
}
