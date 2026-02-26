using System;
using Xui.Core.Abstract;
using Xui.Core.Animation;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
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
public class ScrollView : View
{
    private View? content;
    private nfloat scrollOffsetY;
    private nfloat scrollOffsetX;
    private nfloat contentHeight;       // captured in MeasureCore
    private nfloat contentWidth;        // captured in MeasureCore
    private nfloat viewportHeight;      // captured in ArrangeCore
    private nfloat viewportWidth;       // captured in ArrangeCore

    // Drag tracking
    private bool isDragging;
    private bool isScrollGesture;       // true after delta exceeds ScrollThreshold
    private Point dragStartPos;
    private Point lastPointerPos;
    private long lastPointerTick;       // Environment.TickCount64 (ms)
    private nfloat dragVelocityY;       // pts/sec, positive = scroll down (offset increases)
    private nfloat dragVelocityX;       // pts/sec, positive = scroll right (offset increases)

    // Fling animation
    private ExponentialDecayCurve? flingCurveY;
    private nfloat? pendingFlingVelocityY; // set on Up, initialized in AnimateCore with accurate startTime
    private ExponentialDecayCurve? flingCurveX;
    private nfloat? pendingFlingVelocityX;

    // Scrollbar appearance
    public nfloat ScrollbarWidth { get; set; } = 3f;
    public nfloat ScrollbarEndInset { get; set; } = 4f;

    /// <summary>
    /// Gets or sets which axis (or axes) this scroll view responds to.
    /// </summary>
    public ScrollDirection Direction { get; set; } = ScrollDirection.Vertical;

    private static readonly nfloat ScrollThreshold = 8f; // pts before gesture is recognized as scroll

    /// <summary>
    /// Gets or sets the single child view to scroll.
    /// </summary>
    public View? Content
    {
        get => content;
        set => SetProtectedChild(ref content, value);
    }

    public override int Count => content is not null ? 1 : 0;

    public override View this[int index] => index == 0 && content is not null
        ? content : throw new IndexOutOfRangeException();

    protected override void OnActivate()
    {
        if (this.TryFindParent<RootView>(out var root))
        {
            var r = root.Window.ScreenCornerRadius;
            if (r > 0)
                ScrollbarEndInset = nfloat.Max(ScrollbarEndInset, r / 3f);
        }
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        contentWidth = 0;
        contentHeight = 0;

        if (content != null)
        {
            var measureSize = Direction switch
            {
                ScrollDirection.Horizontal => new Size(nfloat.PositiveInfinity, available.Height),
                ScrollDirection.Both       => new Size(nfloat.PositiveInfinity, nfloat.PositiveInfinity),
                _                          => new Size(available.Width, nfloat.PositiveInfinity)
            };
            var desired = content.Measure(measureSize, context);
            contentWidth = desired.Width;
            contentHeight = desired.Height;
        }

        nfloat w = nfloat.IsFinite(available.Width)  ? available.Width  : contentWidth;
        nfloat h = nfloat.IsFinite(available.Height) ? available.Height : contentHeight;
        return (w, h);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        viewportHeight = rect.Height;
        viewportWidth = rect.Width;
        ClampScrollOffset();

        if (content != null)
        {
            var contentRect = new Rect(rect.X - scrollOffsetX, rect.Y - scrollOffsetY, contentWidth, contentHeight);
            content.Arrange(contentRect, context, new Size(contentWidth, contentHeight));
        }
    }

    private void ClampScrollOffset()
    {
        scrollOffsetY = nfloat.Clamp(scrollOffsetY, 0, MaxScrollOffsetY);
        scrollOffsetX = nfloat.Clamp(scrollOffsetX, 0, MaxScrollOffsetX);
    }

    private nfloat MaxScrollOffsetY => nfloat.Max(0, contentHeight - viewportHeight);
    private nfloat MaxScrollOffsetX => nfloat.Max(0, contentWidth - viewportWidth);

    public override bool HitTest(Point point)
    {
        if (!this.Frame.Contains(point)) return false;
        for (int i = this.Count - 1; i >= 0; i--)
            if (this[i].HitTest(point))
                return true;
        return true; // ScrollView always captures input within its bounds
    }

    protected override void RenderCore(IContext context)
    {
        context.Save();

        context.BeginPath();
        context.Rect(this.Frame);
        context.Clip();

        content?.Render(context);
        DrawScrollbarIndicatorV(context);
        DrawScrollbarIndicatorH(context);

        context.Restore();
        // base.RenderCore intentionally NOT called — would re-render content
    }

    private void DrawScrollbarIndicatorV(IContext context)
    {
        if (Direction == ScrollDirection.Horizontal) return;
        if (MaxScrollOffsetY <= 0) return;

        nfloat trackH = viewportHeight - ScrollbarEndInset * 2;
        nfloat ratio = viewportHeight / contentHeight;
        nfloat barH = nfloat.Max(trackH * ratio, 20f);
        nfloat scrollProgress = scrollOffsetY / MaxScrollOffsetY;
        nfloat barTop = this.Frame.Y + ScrollbarEndInset + (trackH - barH) * scrollProgress;
        nfloat barX = this.Frame.Right - ScrollbarWidth - 2f;

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
        nfloat scrollProgress = scrollOffsetX / MaxScrollOffsetX;
        nfloat barLeft = this.Frame.X + ScrollbarEndInset + (trackW - barW) * scrollProgress;
        nfloat barY = this.Frame.Bottom - ScrollbarWidth - 2f;

        context.SetFill(new Color(0f, 0f, 0f, 0.35f));
        context.BeginPath();
        context.RoundRect(new Rect(barLeft, barY, barW, ScrollbarWidth), ScrollbarWidth / 2);
        context.Fill(FillRule.NonZero);
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;

        switch (e.Type)
        {
            case PointerEventType.Down:
                isDragging = true;
                isScrollGesture = false;
                dragStartPos = e.State.Position;
                lastPointerPos = e.State.Position;
                lastPointerTick = Environment.TickCount64;
                dragVelocityY = 0;
                dragVelocityX = 0;
                flingCurveY = null;
                flingCurveX = null;
                pendingFlingVelocityY = null;
                pendingFlingVelocityX = null;
                // Do NOT capture pointer yet — wait for ScrollThreshold
                break;

            case PointerEventType.Move when isDragging:
            {
                var totalDx = (nfloat)(e.State.Position.X - dragStartPos.X);
                var totalDy = (nfloat)(e.State.Position.Y - dragStartPos.Y);

                if (!isScrollGesture && nfloat.Max(nfloat.Abs(totalDx), nfloat.Abs(totalDy)) > ScrollThreshold)
                {
                    isScrollGesture = true;
                    CapturePointer(e.PointerId);
                    lastPointerPos = e.State.Position; // reset for accurate velocity
                    lastPointerTick = Environment.TickCount64;
                }

                if (!isScrollGesture) break;

                var dx = (nfloat)(e.State.Position.X - lastPointerPos.X);
                var dy = (nfloat)(e.State.Position.Y - lastPointerPos.Y);
                var dt = (Environment.TickCount64 - lastPointerTick) / 1000.0;

                if (dt > 0)
                {
                    if (Direction != ScrollDirection.Horizontal)
                    {
                        nfloat sampleY = (nfloat)(-dy / dt);
                        dragVelocityY = dragVelocityY * 0.6f + sampleY * 0.4f;
                    }
                    if (Direction != ScrollDirection.Vertical)
                    {
                        nfloat sampleX = (nfloat)(-dx / dt);
                        dragVelocityX = dragVelocityX * 0.6f + sampleX * 0.4f;
                    }
                }

                if (Direction != ScrollDirection.Horizontal)
                    scrollOffsetY = nfloat.Clamp(scrollOffsetY - dy, 0, MaxScrollOffsetY);
                if (Direction != ScrollDirection.Vertical)
                    scrollOffsetX = nfloat.Clamp(scrollOffsetX - dx, 0, MaxScrollOffsetX);

                lastPointerPos = e.State.Position;
                lastPointerTick = Environment.TickCount64;

                InvalidateArrange();
                InvalidateRender();
                break;
            }

            case PointerEventType.Up when isDragging:
                isDragging = false;

                if (isScrollGesture)
                {
                    isScrollGesture = false;
                    ReleasePointer(e.PointerId);

                    if (Direction != ScrollDirection.Horizontal && nfloat.Abs(dragVelocityY) > 50f)
                    {
                        pendingFlingVelocityY = dragVelocityY;
                        RequestAnimationFrame();
                    }
                    if (Direction != ScrollDirection.Vertical && nfloat.Abs(dragVelocityX) > 50f)
                    {
                        pendingFlingVelocityX = dragVelocityX;
                        RequestAnimationFrame();
                    }
                }
                break;

            case PointerEventType.Cancel when isDragging:
                isDragging = false;
                if (isScrollGesture)
                {
                    isScrollGesture = false;
                    ReleasePointer(e.PointerId);
                }
                flingCurveY = null;
                flingCurveX = null;
                pendingFlingVelocityY = null;
                pendingFlingVelocityX = null;
                break;
        }
    }

    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        if (e.Handled) return;

        bool changed = false;

        if (Direction != ScrollDirection.Horizontal && e.Delta.Y != 0)
        {
            scrollOffsetY = nfloat.Clamp(scrollOffsetY - (nfloat)e.Delta.Y / 120f * 80f, 0, MaxScrollOffsetY);
            changed = true;
        }
        if (Direction != ScrollDirection.Vertical && e.Delta.X != 0)
        {
            scrollOffsetX = nfloat.Clamp(scrollOffsetX - (nfloat)e.Delta.X / 120f * 80f, 0, MaxScrollOffsetX);
            changed = true;
        }

        if (!changed) return;

        flingCurveY = null;
        flingCurveX = null;
        pendingFlingVelocityY = null;
        pendingFlingVelocityX = null;
        e.Handled = true;
        InvalidateArrange();
        InvalidateRender();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (pendingFlingVelocityY.HasValue)
        {
            flingCurveY = new ExponentialDecayCurve(
                startTime: current,
                startPosition: scrollOffsetY,
                initialVelocity: pendingFlingVelocityY.Value,
                decayPerSecond: ExponentialDecayCurve.Normal);
            pendingFlingVelocityY = null;
        }

        if (pendingFlingVelocityX.HasValue)
        {
            flingCurveX = new ExponentialDecayCurve(
                startTime: current,
                startPosition: scrollOffsetX,
                initialVelocity: pendingFlingVelocityX.Value,
                decayPerSecond: ExponentialDecayCurve.Normal);
            pendingFlingVelocityX = null;
        }

        bool needsFrame = false;
        bool changed = false;

        if (flingCurveY is { } curveY && !isDragging)
        {
            nfloat newOffset = curveY[current];
            nfloat clamped = nfloat.Clamp(newOffset, 0, MaxScrollOffsetY);
            scrollOffsetY = clamped;
            changed = true;

            if (current < curveY.EndTime && clamped == newOffset)
                needsFrame = true;
            else
                flingCurveY = null;
        }

        if (flingCurveX is { } curveX && !isDragging)
        {
            nfloat newOffset = curveX[current];
            nfloat clamped = nfloat.Clamp(newOffset, 0, MaxScrollOffsetX);
            scrollOffsetX = clamped;
            changed = true;

            if (current < curveX.EndTime && clamped == newOffset)
                needsFrame = true;
            else
                flingCurveX = null;
        }

        if (needsFrame)
            RequestAnimationFrame();

        if (changed)
        {
            InvalidateArrange();
            InvalidateRender();
        }
    }
}
