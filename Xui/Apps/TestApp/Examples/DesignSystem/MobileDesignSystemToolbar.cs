using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Two-button phone toolbar for switching palette and widget-preview pages.</summary>
internal sealed class MobileDesignSystemToolbar(Action<int> selectPage) : View
{
    private int? pressedPage;
    private int? hoverPage;

    public int SelectedPage { get; set; }

    protected override Size MeasureCore(Size available, IMeasureContext context) => new(available.Width, 48);

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse && e.Type is PointerEventType.Enter or PointerEventType.Move)
            hoverPage = GetPage(e.State.Position);
        else if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Leave)
            hoverPage = null;
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            pressedPage = GetPage(e.State.Position);
            CapturePointer(e.PointerId, PointerGestures.Tap);
        }
        else if (phase == EventPhase.Tunnel && e.Type is PointerEventType.Up or PointerEventType.Cancel)
        {
            var page = pressedPage;
            pressedPage = null;
            ReleasePointer(e.PointerId);
            if (page is not null && e.Type == PointerEventType.Up)
                selectPage(page.Value);
        }
        else return;

        InvalidateRender();
    }

    protected override void RenderCore(IContext context)
    {
        var design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        var outline = design?.Colors.OutlineVariant ?? Colors.LightGray;
        context.BeginPath();
        context.RoundRect(Frame, Frame.Height / 2);
        context.SetFill(design?.Colors.Surface.Background ?? Colors.White);
        context.Fill();
        context.BeginPath();
        context.RoundRect(Frame, Frame.Height / 2);
        context.SetStroke(outline);
        context.LineWidth = 1;
        context.Stroke();
        DrawButton(context, 0, "Palette", design);
        DrawButton(context, 1, "Preview", design);
    }

    private void DrawButton(IContext context, int page, string text, IDesignSystem? design)
    {
        var frame = new Rect(Frame.X + 4 + page * (Frame.Width / 2), Frame.Y + 4, Frame.Width / 2 - 8, Frame.Height - 8);
        var selected = SelectedPage == page;
        if (selected || hoverPage == page || pressedPage == page)
        {
            context.BeginPath();
            context.RoundRect(frame, frame.Height / 2);
            context.SetFill(selected ? design?.Colors.Primary.Container ?? Colors.LightGray : design?.Colors.Surface.Container ?? Colors.LightGray);
            context.Fill();
        }
        context.SetFont(new Font(15, "Inter", FontWeight.SemiBold));
        context.TextAlign = TextAlign.Center;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(selected ? design?.Colors.Primary.OnContainer ?? Colors.Black : design?.Colors.Surface.Foreground ?? Colors.Black);
        context.FillText(text, frame.Center);
    }

    private int? GetPage(Point point)
    {
        if (!Frame.Contains(point)) return null;
        return point.X < Frame.Center.X ? 0 : 1;
    }
}
