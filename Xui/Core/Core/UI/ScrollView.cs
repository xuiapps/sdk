using System;
using Xui.Core.Abstract;
using Xui.Core.Animation;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.DI;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

/// <summary>
/// Controls which axis (or axes) a <see cref="ScrollView"/> scrolls along.
/// </summary>
public enum ScrollDirection
{
    /// <summary>Scrolls vertically only (default).</summary>
    Vertical,
    /// <summary>Scrolls horizontally only.</summary>
    Horizontal,
    /// <summary>Scrolls both axes simultaneously (map-panning style).</summary>
    Both
}

/// <summary>
/// A container that clips its content and allows scrolling via pointer drag/fling
/// and scroll wheel input. Supports vertical, horizontal, or both axes.
/// Displays thin overlay scrollbar indicators on the relevant edges.
/// </summary>
public partial class ScrollView : View
{
    private ScrollContent? scrollContent;
    private nfloat scrollOffsetY;
    private nfloat scrollOffsetX;
    private nfloat contentHeight;       // captured in MeasureCore
    private nfloat contentWidth;        // captured in MeasureCore
    private nfloat viewportHeight;      // captured in ArrangeCore
    private nfloat viewportWidth;       // captured in ArrangeCore

    internal Vector ScrollOffset => new(scrollOffsetX, scrollOffsetY);

    // Drag tracking
    private ScrollGestureMode gestureMode;
    private Point dragStartPos;
    private Point lastPointerPos;
    private long lastPointerTick;
    private TimeProvider clock = TimeProvider.System;
    private nfloat dragVelocityY;       // pts/sec, positive = scroll down (offset increases)
    private nfloat dragVelocityX;       // pts/sec, positive = scroll right (offset increases)

    // Negotiation state for the active pointer.
    // - None:               not tracking a pointer
    // - WaitingForTapSteal: a descendant captured ITap; we may steal once movement exceeds ScrollThreshold
    // - WatchingTentative:  a descendant tentatively captured the other axis; we may steal if our axis dominates
    // - Scrolling:          we hold capture (IDrag or tentative) and are translating motion into scroll offset
    // - StandDown:          a descendant captured IDrag — we ignore this pointer entirely
    private enum ScrollGestureMode { None, WaitingForTapSteal, WatchingTentative, Scrolling, StandDown }

    // Single-axis drags yield to the other axis until their direction commits.
    private bool tentative;
    private static readonly nfloat TentativePromoteThreshold = 20f; // pts of dominant-axis travel before promotion

    // Fling animation
    private ExponentialDecayCurve? flingCurveY;
    private nfloat? pendingFlingVelocityY; // set on Up, initialized in AnimateCore with accurate startTime
    private ExponentialDecayCurve? flingCurveX;
    private nfloat? pendingFlingVelocityX;

    private bool elastic;
    private int activePointer = -1;
    private SpringMotionCurve? springX, springY;
    private bool returnX, returnY;
    private bool wheelReturn;
    private TimeSpan? wheelReturnStart;

    private bool SupportsBounce => GetService(typeof(IDeviceInfo)) is IDeviceInfo
        { Platform: DevicePlatform.iOS or DevicePlatform.MacOS, PrefersReducedMotion: false };

    // Scrollbar appearance
    /// <summary>Width of the scrollbar track in points.</summary>
    public nfloat ScrollbarWidth { get; set; } = 3f;
    private nfloat? scrollbarEndInset;
    private nfloat automaticScrollbarEndInset = 4f;

    /// <summary>Inset at each end of the scrollbar. When unset, adapts to the screen corners.</summary>
    public nfloat ScrollbarEndInset
    {
        get => scrollbarEndInset ?? automaticScrollbarEndInset;
        set
        {
            if (scrollbarEndInset == value) return;
            scrollbarEndInset = value;
            InvalidateRender();
        }
    }

    /// <summary>
    /// Gets or sets whether content is clipped and culled to the viewport.
    /// Disable this for a root scroller that is laid out inside a safe area but
    /// is allowed to paint underneath the system areas while it scrolls.
    /// </summary>
    public bool ClipToViewport { get; set; } = true;

    /// <summary>
    /// Gets or sets which axis (or axes) this scroll view responds to.
    /// </summary>
    public new ScrollDirection Direction { get; set; } = ScrollDirection.Vertical;

    private static readonly nfloat ScrollThreshold = 8f; // pts before gesture is recognized as scroll

    /// <summary>
    /// Gets or sets the single child view to scroll.
    /// </summary>
    public View? Content
    {
        get => scrollContent?.Content;
        set
        {
            if (ReferenceEquals(value, scrollContent?.Content))
                return;

            scrollContent?.ClearContent();
            SetProtectedChild(ref scrollContent, value is null ? null : new ScrollContent(this, value));
            scrollContent?.UpdateTransform();
        }
    }

    /// <summary>Number of child views.</summary>
    public override int Count => scrollContent is not null ? 1 : 0;

    /// <summary>Gets the child view at the given index.</summary>
    public override View this[int index] => index == 0 && scrollContent is not null
        ? scrollContent : throw new IndexOutOfRangeException();

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        automaticScrollbarEndInset = 4f;
        if (this.TryFindParent<RootView>(out var root))
        {
            var r = root.Window.ScreenCornerRadius;
            if (r > 0)
                automaticScrollbarEndInset = nfloat.Max(4f, r / 3f);
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        contentWidth = 0;
        contentHeight = 0;

        if (scrollContent != null)
        {
            var measureSize = Direction switch
            {
                ScrollDirection.Horizontal => new Size(nfloat.PositiveInfinity, available.Height),
                ScrollDirection.Both       => new Size(nfloat.PositiveInfinity, nfloat.PositiveInfinity),
                _                          => new Size(available.Width, nfloat.PositiveInfinity)
            };
            var desired = scrollContent.Measure(measureSize, context);
            contentWidth = desired.Width;
            contentHeight = desired.Height;

            // A one-axis scroller is a viewport-sized surface on its other axis.
            // The content may naturally measure narrower (for example, a vertical
            // stack of labels), but arranging its host at that width makes the
            // scroll-coordinate space shrink-wrap instead of stretching to the
            // viewport.
            if (Direction == ScrollDirection.Vertical && nfloat.IsFinite(available.Width))
                contentWidth = nfloat.Max(contentWidth, available.Width);
            else if (Direction == ScrollDirection.Horizontal && nfloat.IsFinite(available.Height))
                contentHeight = nfloat.Max(contentHeight, available.Height);
        }

        nfloat w = nfloat.IsFinite(available.Width)  ? available.Width  : contentWidth;
        nfloat h = nfloat.IsFinite(available.Height) ? available.Height : contentHeight;
        return (w, h);
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        viewportHeight = rect.Height;
        viewportWidth = rect.Width;
        // A layout pass during a pull must not discard its presentation offset.
        if (activePointer == -1 && springX is null && springY is null && !returnX && !returnY)
            ClampScrollOffset();
        scrollContent?.UpdateTransform();

        if (scrollContent != null)
        {
            var contentRect = new Rect(0, 0, contentWidth, contentHeight);
            scrollContent.Arrange(contentRect, context, new Size(contentWidth, contentHeight));
        }
    }

    private void ClampScrollOffset()
    {
        scrollOffsetY = nfloat.Clamp(scrollOffsetY, 0, MaxScrollOffsetY);
        scrollOffsetX = nfloat.Clamp(scrollOffsetX, 0, MaxScrollOffsetX);
    }

    private nfloat MaxScrollOffsetY => nfloat.Max(0, contentHeight - viewportHeight);
    private nfloat MaxScrollOffsetX => nfloat.Max(0, contentWidth - viewportWidth);

    /// <inheritdoc/>
    public override Point TransformPoint(Point point) => point - new Vector(this.Frame.X, this.Frame.Y);

    /// <inheritdoc/>
    public override Point InverseTransformPoint(Point point) => point + this.Frame.TopLeft;

    /// <inheritdoc/>
    public override Rect TransformRect(Rect rect) =>
        new(rect.X - this.Frame.X, rect.Y - this.Frame.Y, rect.Width, rect.Height);

    /// <inheritdoc/>
    public override Rect InverseTransformRect(Rect rect) =>
        new(rect.X + this.Frame.X, rect.Y + this.Frame.Y, rect.Width, rect.Height);

    /// <inheritdoc/>
    /// <inheritdoc/>
    public override bool HitTest(Point point)
    {
        return new Rect(0, 0, this.Frame.Width, this.Frame.Height).Contains(point);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        context.Save();
        context.Translate(this.Frame.TopLeft);

        // RenderCullingFrame is supplied by the window/ancestor in this
        // ScrollView's local coordinates. ClipToViewport may narrow it, but
        // disabling the viewport clip must never discard that external bound.
        Rect? contentCullingFrame = this.RenderCullingFrame;

        if (ClipToViewport)
        {
            var viewport = new Rect(0, 0, this.Frame.Width, this.Frame.Height);
            context.BeginPath();
            context.Rect(viewport);
            context.Clip();
            contentCullingFrame = contentCullingFrame is { } externalCulling
                ? Intersect(externalCulling, viewport)
                : viewport;
        }

        if (scrollContent is not null)
        {
            if (contentCullingFrame is { } cullingFrame)
            {
                if (cullingFrame.Width > 0 && cullingFrame.Height > 0)
                    this.RenderChild(context, scrollContent, cullingFrame);
            }
            else
            {
                this.RenderChild(context, scrollContent);
            }
        }

        DrawScrollbarIndicatorV(context);
        DrawScrollbarIndicatorH(context);

        context.Restore();
        // base.RenderCore intentionally NOT called — would re-render content
    }

    private static Rect Intersect(Rect first, Rect second)
    {
        var left = nfloat.Max(first.Left, second.Left);
        var top = nfloat.Max(first.Top, second.Top);
        var right = nfloat.Min(first.Right, second.Right);
        var bottom = nfloat.Min(first.Bottom, second.Bottom);
        return new Rect(left, top, nfloat.Max(0, right - left), nfloat.Max(0, bottom - top));
    }

    private void DrawScrollbarIndicatorV(IContext context)
    {
        if (Direction == ScrollDirection.Horizontal) return;
        if (MaxScrollOffsetY <= 0) return;

        nfloat trackH = viewportHeight - ScrollbarEndInset * 2;
        nfloat ratio = viewportHeight / contentHeight;
        nfloat barH = nfloat.Max(trackH * ratio, 20f);
        nfloat scrollProgress = nfloat.Clamp(scrollOffsetY / MaxScrollOffsetY, 0, 1);
        barH = nfloat.Max(8, barH - nfloat.Abs(scrollOffsetY - nfloat.Clamp(scrollOffsetY, 0, MaxScrollOffsetY)));
        nfloat barTop = ScrollbarEndInset + (trackH - barH) * scrollProgress;
        nfloat barX = this.Frame.Width - ScrollbarWidth - 2f;

        context.SetFill(new Color(0f, 0f, 0f, 0.35f));
        context.BeginPath();
        context.RoundRect(new Rect(barX, barTop, ScrollbarWidth, barH), ScrollbarWidth / 2);
        context.Fill(FillRule.NonZero);
    }

    private void DrawScrollbarIndicatorH(IContext context)
    {
        if (Direction == ScrollDirection.Vertical) return;
        if (MaxScrollOffsetX <= 0) return;

        nfloat trackW = viewportWidth - ScrollbarEndInset * 2;
        nfloat ratio = viewportWidth / contentWidth;
        nfloat barW = nfloat.Max(trackW * ratio, 20f);
        nfloat scrollProgress = nfloat.Clamp(scrollOffsetX / MaxScrollOffsetX, 0, 1);
        barW = nfloat.Max(8, barW - nfloat.Abs(scrollOffsetX - nfloat.Clamp(scrollOffsetX, 0, MaxScrollOffsetX)));
        nfloat barLeft = ScrollbarEndInset + (trackW - barW) * scrollProgress;
        nfloat barY = this.Frame.Height - ScrollbarWidth - 2f;

        context.SetFill(new Color(0f, 0f, 0f, 0.35f));
        context.BeginPath();
        context.RoundRect(new Rect(barLeft, barY, barW, ScrollbarWidth), ScrollbarWidth / 2);
        context.Fill(FillRule.NonZero);
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down)
        {
            // Observers may not see Up after an ancestor steals from a button.
            // A new Down starts a new sequence; only an owned drag excludes another pointer.
            if (activePointer != -1 && activePointer != e.PointerId && gestureMode == ScrollGestureMode.Scrolling) return;
            activePointer = e.PointerId;
        }
        else if (e.PointerId != activePointer) return;

        switch (e.Type)
        {
            case PointerEventType.Down:
            {
                // Tunnel ran first — descendants have already had their chance to capture.
                // Inspect what (if anything) they took to decide our negotiation mode.
                var captured = GetCapturedGesture(e.PointerId);

                tentative = false;
                elastic = SupportsBounce;
                clock = GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
                springX = springY = null;
                returnX = returnY = wheelReturn = false;
                dragStartPos = e.State.Position;
                lastPointerPos = e.State.Position;
                lastPointerTick = clock.GetTimestamp();
                dragVelocityY = 0;
                dragVelocityX = 0;
                flingCurveY = null;
                flingCurveX = null;
                pendingFlingVelocityY = null;
                pendingFlingVelocityX = null;

                if (captured == null)
                {
                    // Nobody else wants this pointer — claim it now and scroll on first Move (no threshold).
                    // Single-axis scrollers capture tentatively, so an ancestor on the other axis
                    // can take over when the user's intent reveals itself.
                    gestureMode = ScrollGestureMode.Scrolling;
                    if (Direction == ScrollDirection.Horizontal)
                    {
                        tentative = true;
                        CapturePointer(e.PointerId, PointerGestures.DragHorizontalTentative);
                    }
                    else if (Direction == ScrollDirection.Vertical)
                    {
                        tentative = true;
                        CapturePointer(e.PointerId, PointerGestures.DragVerticalTentative);
                    }
                    else
                    {
                        tentative = false;
                        CapturePointer(e.PointerId, PointerGestures.Drag);
                    }
                }
                else if ((captured is IDragHorizontalTentative && Direction == ScrollDirection.Vertical) ||
                         (captured is IDragVerticalTentative && Direction == ScrollDirection.Horizontal))
                {
                    // The inner scroller is tentative on the other axis.
                    gestureMode = ScrollGestureMode.WatchingTentative;
                }
                else if (captured is ITap)
                {
                    // Tap-style descendant. We may steal capture once movement exceeds ScrollThreshold.
                    gestureMode = ScrollGestureMode.WaitingForTapSteal;
                }
                else
                {
                    // IDrag (color wheel, slider, nested scroll view, …) or unknown gesture marker.
                    // Default to standing down — never steal.
                    gestureMode = ScrollGestureMode.StandDown;
                    activePointer = -1;
                    QueueReturnToBounds();
                }
                break;
            }

            case PointerEventType.Move when gestureMode == ScrollGestureMode.WaitingForTapSteal:
            {
                var totalDx = (nfloat)(e.State.Position.X - dragStartPos.X);
                var totalDy = (nfloat)(e.State.Position.Y - dragStartPos.Y);

                if (nfloat.Max(nfloat.Abs(totalDx), nfloat.Abs(totalDy)) > ScrollThreshold)
                {
                    // Leave drags on the other axis to the ancestor carousel/scroller.
                    if ((Direction == ScrollDirection.Vertical && nfloat.Abs(totalDx) >= nfloat.Abs(totalDy)) ||
                        (Direction == ScrollDirection.Horizontal && nfloat.Abs(totalDy) >= nfloat.Abs(totalDx)))
                    {
                        gestureMode = ScrollGestureMode.StandDown;
                        activePointer = -1;
                        QueueReturnToBounds();
                        break;
                    }
                    // Re-check the current gesture: in nested ScrollView setups, an inner
                    // ScrollView's bubble runs before ours and may have already stolen.
                    // If the pointer is now an IDrag, we must stand down.
                    var current = GetCapturedGesture(e.PointerId);
                    if (current is ITap)
                    {
                        gestureMode = ScrollGestureMode.Scrolling;
                        CapturePointer(e.PointerId, PointerGestures.Drag);
                        lastPointerPos = e.State.Position; // reset for accurate velocity
                        lastPointerTick = clock.GetTimestamp();
                    }
                    else
                    {
                        gestureMode = ScrollGestureMode.StandDown;
                        activePointer = -1;
                        QueueReturnToBounds();
                    }
                }
                break;
            }

            case PointerEventType.Move when gestureMode == ScrollGestureMode.WatchingTentative:
            {
                var totalDx = (nfloat)(e.State.Position.X - dragStartPos.X);
                var totalDy = (nfloat)(e.State.Position.Y - dragStartPos.Y);

                if (nfloat.Max(nfloat.Abs(totalDx), nfloat.Abs(totalDy)) > ScrollThreshold)
                {
                    // Re-check: the inner may have promoted itself to firm IDrag in this same Move.
                    var current = GetCapturedGesture(e.PointerId);
                    if ((current is IDragHorizontalTentative && Direction == ScrollDirection.Vertical && nfloat.Abs(totalDy) > nfloat.Abs(totalDx)) ||
                        (current is IDragVerticalTentative && Direction == ScrollDirection.Horizontal && nfloat.Abs(totalDx) > nfloat.Abs(totalDy)))
                    {
                        // Our axis dominates — steal capture from the tentative inner.
                        gestureMode = ScrollGestureMode.Scrolling;
                        CapturePointer(e.PointerId, PointerGestures.Drag);
                        lastPointerPos = e.State.Position;
                        lastPointerTick = clock.GetTimestamp();
                    }
                    else
                    {
                        // The other axis dominates, or the inner already committed.
                        gestureMode = ScrollGestureMode.StandDown;
                        activePointer = -1;
                        QueueReturnToBounds();
                    }
                }
                break;
            }

            case PointerEventType.Move when gestureMode == ScrollGestureMode.Scrolling:
            {
                if (tentative)
                {
                    var travel = e.State.Position - dragStartPos;
                    if ((Direction == ScrollDirection.Vertical && nfloat.Abs(travel.X) > nfloat.Abs(travel.Y)) ||
                        (Direction == ScrollDirection.Horizontal && nfloat.Abs(travel.Y) > nfloat.Abs(travel.X)))
                        break;
                }
                var dx = (nfloat)(e.State.Position.X - lastPointerPos.X);
                var dy = (nfloat)(e.State.Position.Y - lastPointerPos.Y);
                var dt = clock.GetElapsedTime(lastPointerTick).TotalSeconds;

                var nextY = Direction != ScrollDirection.Horizontal
                    ? DragOffset(scrollOffsetY, -dy, MaxScrollOffsetY, viewportHeight) : scrollOffsetY;
                var nextX = Direction != ScrollDirection.Vertical
                    ? DragOffset(scrollOffsetX, -dx, MaxScrollOffsetX, viewportWidth) : scrollOffsetX;
                if (dt >= 0.001)
                {
                    if (Direction != ScrollDirection.Horizontal)
                    {
                        nfloat sampleY = (nextY - scrollOffsetY) / (nfloat)dt;
                        dragVelocityY = dragVelocityY * 0.6f + sampleY * 0.4f;
                    }
                    if (Direction != ScrollDirection.Vertical)
                    {
                        nfloat sampleX = (nextX - scrollOffsetX) / (nfloat)dt;
                        dragVelocityX = dragVelocityX * 0.6f + sampleX * 0.4f;
                    }
                }

                scrollOffsetY = nextY;
                scrollOffsetX = nextX;

                lastPointerPos = e.State.Position;
                lastPointerTick = clock.GetTimestamp();

                // Commit only when our axis dominates; otherwise an ancestor can take over.
                if (tentative)
                {
                    var totalDx = (nfloat)(e.State.Position.X - dragStartPos.X);
                    var totalDy = (nfloat)(e.State.Position.Y - dragStartPos.Y);
                    if ((Direction == ScrollDirection.Horizontal && nfloat.Abs(totalDx) > TentativePromoteThreshold && nfloat.Abs(totalDx) > nfloat.Abs(totalDy)) ||
                        (Direction == ScrollDirection.Vertical && nfloat.Abs(totalDy) > TentativePromoteThreshold && nfloat.Abs(totalDy) > nfloat.Abs(totalDx)))
                    {
                        tentative = false;
                        CapturePointer(e.PointerId, PointerGestures.Drag);
                    }
                }

                InvalidateScrollOffset();
                break;
            }

            case PointerEventType.Up:
            {
                var scrolling = gestureMode == ScrollGestureMode.Scrolling;
                // Clear identity before ReleasePointer synchronously emits LostCapture.
                activePointer = -1;
                gestureMode = ScrollGestureMode.None;
                tentative = false;
                ReleasePointer(e.PointerId);
                var fresh = clock.GetElapsedTime(lastPointerTick).TotalMilliseconds < 100;
                if (scrolling)
                {
                    if (Direction != ScrollDirection.Horizontal)
                        pendingFlingVelocityY = fresh ? nfloat.Clamp(dragVelocityY, -8000, 8000) : 0;
                    if (Direction != ScrollDirection.Vertical)
                        pendingFlingVelocityX = fresh ? nfloat.Clamp(dragVelocityX, -8000, 8000) : 0;
                }
                QueueReturnToBounds();
                if (pendingFlingVelocityX.HasValue || pendingFlingVelocityY.HasValue) RequestAnimationFrame();
                break;
            }
            case PointerEventType.Cancel:
            case PointerEventType.LostCapture:
            {
                activePointer = -1;
                gestureMode = ScrollGestureMode.None;
                tentative = false;
                flingCurveY = flingCurveX = null;
                pendingFlingVelocityY = pendingFlingVelocityX = null;
                if (e.Type == PointerEventType.Cancel) ReleasePointer(e.PointerId);
                QueueReturnToBounds();
                break;
            }
        }
    }

    /// <inheritdoc/>
    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        if (e.Handled || (activePointer != -1 && gestureMode == ScrollGestureMode.Scrolling)) return;
        activePointer = -1;
        elastic = SupportsBounce;
        var changed = false;
        if (Direction != ScrollDirection.Horizontal && e.Delta.Y != 0)
        {
            scrollOffsetY = DragOffset(scrollOffsetY, -e.Delta.Y / 120f * 80f, MaxScrollOffsetY, viewportHeight);
            changed = true;
        }
        if (Direction != ScrollDirection.Vertical && e.Delta.X != 0)
        {
            scrollOffsetX = DragOffset(scrollOffsetX, -e.Delta.X / 120f * 80f, MaxScrollOffsetX, viewportWidth);
            changed = true;
        }
        if (!changed) return;
        flingCurveY = flingCurveX = null;
        pendingFlingVelocityY = pendingFlingVelocityX = null;
        springX = springY = null;
        // Wheel events currently carry no end phase. Rebound after a short quiet
        // interval so a trackpad stream can keep pulling without fighting a spring.
        wheelReturn = elastic;
        wheelReturnStart = null;
        QueueReturnToBounds();
        e.Handled = true;
        InvalidateScrollOffset();
    }

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        var waiting = false;
        if (wheelReturn)
        {
            wheelReturnStart ??= current;
            waiting = (current - wheelReturnStart.Value).TotalMilliseconds < 80;
            if (!waiting) wheelReturn = false;
        }
        if (waiting) RequestAnimationFrame();
        else if (activePointer == -1)
        {
            var oldX = scrollOffsetX;
            var oldY = scrollOffsetY;
            var frameX = AdvanceAxis(ref scrollOffsetX, ref flingCurveX, ref pendingFlingVelocityX,
                ref springX, ref returnX, MaxScrollOffsetX, viewportWidth, current);
            var frameY = AdvanceAxis(ref scrollOffsetY, ref flingCurveY, ref pendingFlingVelocityY,
                ref springY, ref returnY, MaxScrollOffsetY, viewportHeight, current);
            if (frameX || frameY) RequestAnimationFrame();
            if (oldX != scrollOffsetX || oldY != scrollOffsetY) InvalidateScrollOffset();
        }
        base.AnimateCore(previous, current);
    }

    /// <inheritdoc/>
    protected override void OnDeactivate()
    {
        var pointer = activePointer;
        activePointer = -1;
        if (pointer != -1) ReleasePointer(pointer);
        gestureMode = ScrollGestureMode.None;
        tentative = false;
        flingCurveX = flingCurveY = null;
        pendingFlingVelocityX = pendingFlingVelocityY = null;
        springX = springY = null;
        returnX = returnY = wheelReturn = false;
        ClampScrollOffset();
        InvalidateScrollOffset();
        base.OnDeactivate();
    }

    private void InvalidateScrollOffset()
    {
        scrollContent?.UpdateTransform();
        this.InvalidateHitTest();
        this.InvalidateRender();
    }
}
