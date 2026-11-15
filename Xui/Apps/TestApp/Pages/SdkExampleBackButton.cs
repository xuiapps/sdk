using Xui.Core.Canvas;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using static Xui.Core.Canvas.Colors;
using Xui.Core.Abstract;

namespace Xui.Apps.TestApp.Pages;

public class SdkExampleBackButton : Label
{
    bool hover = false;
    bool pressed = false;

    public SdkExampleBackButton()
    {
        this.Id = "Back";
        this.Text = "< Back";
        this.FontFamily = "Inter";
    }

    private void NavigateBack()
    {
        this.TryFindExampleNavigation(out var navigation);
        navigation?.PopExample();
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Enter)
        {
            hover = true;
            this.InvalidateRender();
        }
        else if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Leave)
        {
            hover = false;
            this.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            this.CapturePointer(e.PointerId, PointerGestures.Tap);
            pressed = true;
            this.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
        {
            var wasPressed = pressed;
            pressed = false;
            this.ReleasePointer(e.PointerId);
            if (wasPressed)
                this.NavigateBack();
            this.InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }

    protected override void RenderCore(IContext context)
    {
        if (this.pressed)
        {
            context.SetFill(Yellow);
            context.FillRect(this.Frame);
        }
        else if (this.hover)
        {
            context.SetFill(LightGray);
            context.FillRect(this.Frame);
        }

        base.RenderCore(context);
    }
}
