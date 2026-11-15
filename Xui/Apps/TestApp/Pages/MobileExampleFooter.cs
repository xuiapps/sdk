using Xui.Apps.TestApp.Examples;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Pages;

/// <summary>Card-like section navigator shown at the bottom of mobile examples.</summary>
internal sealed class MobileExampleFooter : View
{
    private enum Segment { Previous, Current, Next }

    private readonly IExampleSectionNavigator sections;
    private readonly Action toggleMenu;
    private Segment? hover;
    private Segment? pressed;

    public MobileExampleFooter(IExampleSectionNavigator sections, Action toggleMenu)
    {
        this.sections = sections;
        this.toggleMenu = toggleMenu;
        sections.SectionChanged += OnSectionChanged;
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) => new(available.Width, 48);

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse && e.Type is PointerEventType.Enter or PointerEventType.Move)
        {
            hover = GetSegment(e.State.Position);
            InvalidateRender();
            return;
        }

        if (e.State.PointerType == PointerType.Mouse && e.Type == PointerEventType.Leave)
        {
            hover = null;
            pressed = null;
            InvalidateRender();
            return;
        }

        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            pressed = GetSegment(e.State.Position);
            CapturePointer(e.PointerId, PointerGestures.Tap);
            InvalidateRender();
            return;
        }

        if (phase != EventPhase.Tunnel || e.Type is not (PointerEventType.Up or PointerEventType.Cancel))
            return;

        var pressedSegment = pressed;
        pressed = null;
        ReleasePointer(e.PointerId);
        if (pressedSegment is not null && e.Type == PointerEventType.Up)
        {
            switch (pressedSegment.Value)
            {
                case Segment.Previous:
                    sections.SelectSection(sections.SelectedSectionIndex - 1);
                    break;
                case Segment.Next:
                    sections.SelectSection(sections.SelectedSectionIndex + 1);
                    break;
                case Segment.Current:
                    toggleMenu();
                    break;
            }
        }
        InvalidateRender();
    }

    protected override void RenderCore(IContext context)
    {
        var design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        var outline = design?.Colors.OutlineVariant ?? Colors.LightGray;
        var foreground = design?.Colors.Surface.Foreground ?? Colors.Black;
        var previous = GetFrame(Segment.Previous);
        var current = GetFrame(Segment.Current);
        var next = GetFrame(Segment.Next);

        context.BeginPath();
        context.RoundRect(this.Frame, this.Frame.Height / 2);
        context.SetFill(design?.Colors.Surface.Background ?? Colors.White);
        context.Fill();
        context.BeginPath();
        context.RoundRect(this.Frame, this.Frame.Height / 2);
        context.SetStroke(outline);
        context.LineWidth = 1;
        context.Stroke();
        RenderButton(context, previous, Segment.Previous, design, outline);
        RenderButton(context, current, Segment.Current, design, outline);
        RenderButton(context, next, Segment.Next, design, outline);

        DrawChevron(context, previous.Center, pointsRight: false, foreground);
        DrawChevron(context, next.Center, pointsRight: true, foreground);

        context.SetFont(new Font(15, "Inter", FontWeight.SemiBold));
        context.TextAlign = TextAlign.Center;
        context.TextBaseline = TextBaseline.Middle;
        context.SetFill(foreground);
        context.FillText(sections.GetSectionName(sections.SelectedSectionIndex), current.Center);
    }

    private Rect GetFrame(Segment segment) => segment switch
    {
        Segment.Previous => new Rect(this.Frame.X + 4, this.Frame.Y + 4, this.Frame.Height - 8, this.Frame.Height - 8),
        Segment.Next => new Rect(this.Frame.Right - this.Frame.Height + 4, this.Frame.Y + 4, this.Frame.Height - 8, this.Frame.Height - 8),
        _ => new Rect(this.Frame.X + this.Frame.Height, this.Frame.Y + 4, this.Frame.Width - this.Frame.Height * 2, this.Frame.Height - 8),
    };

    private Segment? GetSegment(Point position)
    {
        foreach (var segment in new[] { Segment.Previous, Segment.Current, Segment.Next })
            if (GetFrame(segment).Contains(position))
                return segment;
        return null;
    }

    private void RenderButton(IContext context, Rect frame, Segment segment, IDesignSystem? design, Color outline)
    {
        var isPressed = pressed == segment;
        var isHover = hover == segment;
        if (!isPressed && !isHover)
            return;

        context.BeginPath();
        context.RoundRect(frame, frame.Height / 2);
        context.SetFill(isPressed
            ? design?.Colors.OutlineVariant ?? Colors.LightGray
            : isHover ? design?.Colors.Surface.Container ?? Colors.LightGray
            : design?.Colors.Surface.Background ?? Colors.White);
        context.Fill();
    }

    private static void DrawChevron(IContext context, Point center, bool pointsRight, Color color)
    {
        var direction = pointsRight ? 1 : -1;
        context.BeginPath();
        context.MoveTo(center + (-direction * 2.5f, -4.5f));
        context.LineTo(center + (direction * 2.5f, 0));
        context.LineTo(center + (-direction * 2.5f, 4.5f));
        context.SetStroke(color);
        context.LineWidth = 1.5f;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        context.Stroke();
    }

    private void OnSectionChanged() => InvalidateRender();
}
