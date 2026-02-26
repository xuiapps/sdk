using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Debug;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

public class RootView : View, IContent
{
    private View? content;
    private View? focusedView;
    private Point lastMousePosition;

    public EventRouter EventRouter { get; }

    public Window Window { get; }

    public View? FocusedView
    {
        get => this.focusedView;
        set
        {
            if (this.focusedView == value)
                return;

            var previous = this.focusedView;
            this.focusedView = value;

            previous?.OnBlur();
            value?.OnFocus();

            this.Window.Invalidate();
        }
    }

    public View? Content
    {
        get => this.content;
        set => this.SetProtectedChild(ref this.content, value);
    }

    public override int Count => this.Content is not null ? 1 : 0;

    public override View this[int index] => index == 0 && this.Content is not null ? this.Content : throw new IndexOutOfRangeException();

    public RootView(Window window)
    {
        this.Window = window;
        this.EventRouter = new EventRouter(this);
        var attachEvent = new AttachEventRef();
        AttachSubtree(this, ref attachEvent);
        ActivateSubtree(this);
    }

    void IContent.OnMouseDown(ref MouseDownEventRef e)
    {
        lastMousePosition = e.Position;
        this.EventRouter.Dispatch(ref e);
    }

    void IContent.OnMouseMove(ref MouseMoveEventRef e)
    {
        lastMousePosition = e.Position;
        this.EventRouter.Dispatch(ref e);
    }

    void IContent.OnMouseUp(ref MouseUpEventRef e)
    {
        this.EventRouter.Dispatch(ref e);
    }

    void IContent.OnKeyDown(ref KeyEventRef e)
    {
        if (e.Key == VirtualKey.Tab)
        {
            this.MoveFocus(e.Shift ? -1 : 1);
            e.Handled = true;
            return;
        }

        this.focusedView?.OnKeyDown(ref e);
    }

    void IContent.OnChar(ref KeyEventRef e)
    {
        this.focusedView?.OnChar(ref e);
    }

    void IContent.OnScrollWheel(ref ScrollWheelEventRef e)
    {
        this.EventRouter.Dispatch(ref e, lastMousePosition);
    }

    void IContent.OnTouch(ref TouchEventRef e)
    {
        this.EventRouter.Dispatch(ref e);
    }

    void IContent.OnAnimationFrame(ref FrameEventRef e)
    {
        if ((this.Flags & (ViewFlags.Animated | ViewFlags.DescendantAnimated)) != 0)
        {
            this.Animate(e.Previous, e.Next);
        }
    }

    void IContent.Update(ref RenderEventRef @event, IContext context)
    {
        var instruments = Runtime.CurrentInstruments;
        var rect = @event.Rect;
        using var _ = instruments.Trace(Scope.Rendering, LevelOfDetail.Essential,
            $"RootView.Update Rect({rect.X:F1}, {rect.Y:F1}, {rect.Width:F1}, {rect.Height:F1})");

        this.Update(new LayoutGuide()
        {
            Anchor = @event.Rect.TopLeft,
            PreviousTime = @event.Frame.Previous,
            CurrentTime = @event.Frame.Next,
            Pass =
                LayoutGuide.LayoutPass.Measure |
                LayoutGuide.LayoutPass.Arrange |
                LayoutGuide.LayoutPass.Render,
            AvailableSize = @event.Rect.Size,
            MeasureContext = context,
            XAlign = LayoutGuide.Align.Start,
            YAlign = LayoutGuide.Align.Start,
            XSize = LayoutGuide.SizeTo.Exact,
            YSize = LayoutGuide.SizeTo.Exact,
            RenderContext = context,
            Instruments = instruments,
        });

        instruments.DumpVisualTree(this, LevelOfDetail.Diagnostic);
    }

    private void MoveFocus(int direction)
    {
        View? first = null, last = null, prev = null, next = null;
        bool foundCurrent = false;
        FindFocusNeighbors(this, this.focusedView, ref first, ref last, ref prev, ref next, ref foundCurrent);

        if (first == null)
            return;

        this.FocusedView = direction > 0
            ? next ?? first   // forward: next, or wrap to first
            : prev ?? last;   // backward: prev, or wrap to last
    }

    private static void FindFocusNeighbors(
        View view, View? current,
        ref View? first, ref View? last, ref View? prev, ref View? next, ref bool foundCurrent)
    {
        if (view.Focusable)
        {
            first ??= view;
            last = view;

            if (view == current)
                foundCurrent = true;
            else if (!foundCurrent)
                prev = view;
            else if (next == null)
                next = view;
        }

        for (int i = 0; i < view.Count; i++)
            FindFocusNeighbors(view[i], current, ref first, ref last, ref prev, ref next, ref foundCurrent);
    }

    /// <inheritdoc/>
    public override object? GetService(Type serviceType) =>
        this.Window.GetService(serviceType);

    protected override void OnChildRenderChanged(View child)
    {
        base.OnChildRenderChanged(child);
        ((IContent)this).Invalidate();
    }

    void IContent.Invalidate()
    {
        this.Window.Invalidate();
    }
}
