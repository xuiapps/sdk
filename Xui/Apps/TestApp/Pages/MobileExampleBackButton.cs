using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages;

/// <summary>A compact, iOS-style chevron that returns to the mobile example menu.</summary>
public sealed class MobileExampleBackButton : View
{
    private static readonly Font Font = new(16, "Inter", FontWeight.Medium);
    private bool hover;
    private bool pressed;

    public MobileExampleBackButton() => Id = "Back";

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Enter)
        {
            hover = true;
            InvalidateRender();
            return;
        }

        if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Leave)
        {
            hover = false;
            pressed = false;
            InvalidateRender();
            return;
        }

        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            CapturePointer(e.PointerId, PointerGestures.Tap);
            pressed = true;
            InvalidateRender();
            return;
        }

        if (phase == EventPhase.Tunnel && e.Type is PointerEventType.Up or PointerEventType.Cancel)
        {
            var wasPressed = pressed;
            pressed = false;
            ReleasePointer(e.PointerId);
            if (wasPressed && e.Type == PointerEventType.Up)
            {
                this.TryFindExampleNavigation(out var navigation);
                navigation?.PopExample();
            }
            InvalidateRender();
        }
    }

    protected override void RenderCore(IContext context)
    {
        var design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        var foreground = design?.Colors.Surface.Foreground ?? Colors.Black;
        if (hover || pressed)
        {
            context.BeginPath();
            var background = this.Frame.Expand(-4);
            context.RoundRect(background, background.Height / 2);
            context.SetFill(pressed
                ? design?.Colors.OutlineVariant ?? Colors.LightGray
                : design?.Colors.Surface.Container ?? Colors.LightGray);
            context.Fill();
        }

        var chevronCenter = new Point(this.Frame.X + 18, this.Frame.Center.Y);
        context.BeginPath();
        context.MoveTo(chevronCenter + (2.5f, -5f));
        context.LineTo(chevronCenter + (-2.5f, 0));
        context.LineTo(chevronCenter + (2.5f, 5f));
        context.SetStroke(foreground);
        context.LineWidth = 1.75f;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        context.Stroke();

        context.SetFont(Font);
        context.TextAlign = TextAlign.Left;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(foreground);
        context.FillText("Back", new Point(this.Frame.X + 31, this.Frame.Center.Y));
    }
}
