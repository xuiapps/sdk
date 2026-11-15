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
    private readonly List<PopupOverlay> overlays = new();
    private readonly InstrumentedMeasureContext? instrumentedMeasureContext;

    /// <summary>The render-surface instruments that own this visual tree, if instrumentation is enabled.</summary>
    public IRenderSurfaceInstrumentsSink? RenderSurfaceInstruments => this.Window.RenderSurfaceInstruments;

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

            (this.Window.GetService(typeof(Xui.Core.Actual.ITextInputSession)) as Xui.Core.Actual.ITextInputSession)
                ?.SetClient(value as ITextEditingClient);

            this.Window.RequireKeyboard = this.focusedView?.RequiresKeyboard ?? false;

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

        var instrumentsAccessor = new InstrumentsAccessor(this.RenderSurfaceInstruments);
        var attachEvent = new AttachEventRef
        {
            Instruments = instrumentsAccessor,
        };
        if (this.RenderSurfaceInstruments is not null)
            this.instrumentedMeasureContext = new InstrumentedMeasureContext(instrumentsAccessor);
        AttachSubtree(this, ref attachEvent);
        ActivateSubtree(this);
    }

    void IContent.OnMouseDown(ref MouseDownEventRef e)
    {
        lastMousePosition = e.Position;

        // If the click is inside any visible overlay, prevent it from reaching the underlying UI.
        if (overlays.Count > 0 && IsPointInsideAnyOverlay(e.Position))
            return;

        if (overlays.Count > 0)
            DismissOverlaysOutside(e.Position);

        this.EventRouter.Dispatch(ref e);
    }

    /// <summary>
    /// Returns true if the given position is inside any visible popup overlay.
    /// This prevents mouse events from "clicking through" overlays into underlying views.
    /// </summary>
    private bool IsPointInsideAnyOverlay(Point position)
    {
        foreach (var overlay in overlays)
        {
            if (!overlay.IsVisible)
                continue;

            if (overlay.Frame.Contains(position))
                return true;
        }

        return false;
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
        this.EventRouter.Dispatch(ref e, e.HasPosition ? e.Position : lastMousePosition);
    }

    void IContent.OnTouch(ref TouchEventRef e)
    {
        this.EventRouter.Dispatch(ref e);
    }

    void IContent.OnAnimationFrame(ref FrameEventRef e)
    {
        if ((this.Flags & (ViewFlags.Animated | ViewFlags.DescendantAnimated)) != 0)
        {
            this.Instruments.BeginFrame();
            this.Animate(e.Previous, e.Next);
        }
    }

    void IContent.Update(ref RenderEventRef @event)
    {
        var context = @event.Context;
        var instruments = this.Instruments;
        instruments.BeginFrame();
        IMeasureContext measureContext = context;
        if (instrumentedMeasureContext is not null)
        {
            instrumentedMeasureContext.SetInner(context);
            measureContext = instrumentedMeasureContext;
        }
        var rect = @event.Rect;
        using var _ = instruments.Trace(Scope.Rendering, LevelOfDetail.Essential,
            $"RootView.Update Rect({rect.X:F1}, {rect.Y:F1}, {rect.Width:F1}, {rect.Height:F1})");

        var frame = new LayoutFrameContext(
            @event.Frame.Previous,
            @event.Frame.Next,
            measureContext,
            context,
            instruments);
        var constraints = new ArrangeConstraints(@event.Rect.Size, default, @event.Rect.TopLeft);
        var update = new LayoutUpdate(
            LayoutPass.Measure | LayoutPass.Arrange | LayoutPass.Render,
            new MeasureConstraints(@event.Rect.Size, LayoutSizeMode.Exact, LayoutSizeMode.Exact),
            constraints);
        LayoutMeasurements measurements = default;
        try
        {
            this.Update(in frame, in update, ref measurements);

            // Render in-window popup overlays on top of all content
            for (int i = 0; i < overlays.Count; i++)
                overlays[i].Render(in frame);

            instruments.DumpVisualTree(this, LevelOfDetail.Diagnostic);
        }
        finally
        {
            instruments.EndFrame();
        }
    }

    private void MoveFocus(int direction)
    {
        if (FindFocusNeighbor(direction, wrap: true) is { } next)
            this.FocusedView = next;
    }

    private View? FindFocusNeighbor(int direction, bool wrap)
    {
        View? first = null, last = null, prev = null, next = null;
        bool foundCurrent = false;
        FindFocusNeighbors(this, this.focusedView, ref first, ref last, ref prev, ref next, ref foundCurrent);

        if (this.focusedView is null) return direction > 0 ? first : last;
        return direction > 0
            ? next ?? (wrap ? first : null)
            : prev ?? (wrap ? last : null);
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
    View? IFocus.GetNext(bool wrap) => FindFocusNeighbor(+1, wrap);
    View? IFocus.GetPrevious(bool wrap) => FindFocusNeighbor(-1, wrap);

    internal void AddOverlay(PopupOverlay overlay)
    {
        overlays.Add(overlay);
        ((IContent)this).Invalidate();
    }

    internal void RemoveOverlay(PopupOverlay overlay)
    {
        overlays.Remove(overlay);
        ((IContent)this).Invalidate();
    }

    private void DismissOverlaysOutside(Point point)
    {
        for (int i = overlays.Count - 1; i >= 0; i--)
        {
            if (!overlays[i].ContainsPoint(point))
                overlays[i].Close();
        }
    }

    /// <inheritdoc/>
    public override object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IFocus)) return this;
        if (serviceType == typeof(IViewInstrumentsSink)) return this.RenderSurfaceInstruments;
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
