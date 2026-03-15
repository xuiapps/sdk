using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Debug;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

/// <summary>
/// The root container view that bridges a <see cref="Window"/> with its UI content hierarchy.
/// </summary>
public class RootView : View, IContent, IFocus
{
    private View? content;
    private View? focusedView;
    private Point lastMousePosition;

    /// <summary>The input event router for this view tree.</summary>
    public EventRouter EventRouter { get; }

    /// <summary>The window that owns this root view.</summary>
    public Window Window { get; }

    /// <summary>The currently focused view within this tree, or null if none.</summary>
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

    /// <summary>Gets or sets the single content view hosted by this root view.</summary>
    public View? Content
    {
        get => this.content;
        set => this.SetProtectedChild(ref this.content, value);
    }

    /// <summary>The number of child views (always 1 when Content is set, else 0).</summary>
    public override int Count => this.Content is not null ? 1 : 0;

    /// <summary>Gets the child view at the given index.</summary>
    public override View this[int index] => index == 0 && this.Content is not null ? this.Content : throw new IndexOutOfRangeException();

    /// <summary>Initializes a new RootView for the given window.</summary>
    /// <param name="window">The window that owns this root view.</param>
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
        var instruments = this.Instruments;
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

    void IFocus.Next() => MoveFocus(+1);
    void IFocus.Previous() => MoveFocus(-1);

    /// <inheritdoc/>
    public override object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IFocus)) return this;
        return this.Window.GetService(serviceType);
    }

    /// <summary>Called when a child view signals that its rendered state has changed.</summary>
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
