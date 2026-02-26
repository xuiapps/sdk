using System;
using Xui.Core.Abstract;
using Xui.Core.Animation;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

/// <summary>
/// A container that clips its content and allows vertical scrolling via pointer drag/fling
/// and scroll wheel input. Displays a thin overlay scrollbar indicator on the right edge.
/// </summary>
public class ScrollView : View
{
    private View? content;
    private nfloat scrollOffsetY;
    private nfloat contentHeight;       // captured in MeasureCore
    private nfloat contentWidth;        // captured in MeasureCore
    private nfloat viewportHeight;      // captured in ArrangeCore

    // Drag tracking
    private bool isDragging;
    private bool isScrollGesture;       // true after delta exceeds ScrollThreshold
    private Point dragStartPos;
    private Point lastPointerPos;
    private long lastPointerTick;       // Environment.TickCount64 (ms)
    private nfloat dragVelocity;        // pts/sec, positive = scroll down (offset increases)

    // Fling animation
    private ExponentialDecayCurve? flingCurve;
    private nfloat? pendingFlingVelocity; // set on Up, initialized in AnimateCore with accurate startTime

    // Scrollbar appearance
    public nfloat ScrollbarWidth { get; set; } = 3f;
    public nfloat ScrollbarEndInset { get; set; } = 4f;

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
            var desired = content.Measure((available.Width, nfloat.PositiveInfinity), context);
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
        ClampScrollOffset();

        if (content != null)
        {
            var contentRect = new Rect(rect.X, rect.Y - scrollOffsetY, rect.Width, contentHeight);
            content.Arrange(contentRect, context, new Size(contentWidth, contentHeight));
        }
    }

    private void ClampScrollOffset()
    {
        scrollOffsetY = nfloat.Clamp(scrollOffsetY, 0, MaxScrollOffset);
    }

    private nfloat MaxScrollOffset => nfloat.Max(0, contentHeight - viewportHeight);

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
        DrawScrollbarIndicator(context); // overlay inside the clip

        context.Restore();
        // base.RenderCore intentionally NOT called — would re-render content
    }

    private void DrawScrollbarIndicator(IContext context)
    {
        if (MaxScrollOffset <= 0) return;

        nfloat trackH = viewportHeight - ScrollbarEndInset * 2;
        nfloat ratio = viewportHeight / contentHeight;
        nfloat barH = nfloat.Max(trackH * ratio, 20f);
        nfloat scrollProgress = scrollOffsetY / MaxScrollOffset;
        nfloat barTop = this.Frame.Y + ScrollbarEndInset + (trackH - barH) * scrollProgress;
        nfloat barX = this.Frame.Right - ScrollbarWidth - 2f;

        context.SetFill(new Color(0f, 0f, 0f, 0.35f));
        context.BeginPath();
        context.RoundRect(new Rect(barX, barTop, ScrollbarWidth, barH), ScrollbarWidth / 2);
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
                dragVelocity = 0;
                flingCurve = null;
                pendingFlingVelocity = null;
                // Do NOT capture pointer yet — wait for ScrollThreshold
                break;

            case PointerEventType.Move when isDragging:
            {
                var totalDy = e.State.Position.Y - dragStartPos.Y;

                if (!isScrollGesture && nfloat.Abs((nfloat)totalDy) > ScrollThreshold)
                {
                    isScrollGesture = true;
                    CapturePointer(e.PointerId);
                    lastPointerPos = e.State.Position; // reset for accurate velocity
                    lastPointerTick = Environment.TickCount64;
                }

                if (!isScrollGesture) break;

                var dy = (nfloat)(e.State.Position.Y - lastPointerPos.Y);
                var dt = (Environment.TickCount64 - lastPointerTick) / 1000.0;

                if (dt > 0)
                {
                    // positive velocity = scroll down (offset increases = more content visible below)
                    // finger moves up (dy < 0) → sample = -dy/dt > 0 → offset increases ✓
                    nfloat sample = (nfloat)(-dy / dt);
                    dragVelocity = dragVelocity * 0.6f + sample * 0.4f;
                }

                scrollOffsetY = nfloat.Clamp(scrollOffsetY - dy, 0, MaxScrollOffset);
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

                    if (nfloat.Abs(dragVelocity) > 50f)
                    {
                        pendingFlingVelocity = dragVelocity;
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
                flingCurve = null;
                pendingFlingVelocity = null;
                break;
        }
    }

    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        if (e.Handled) return;
        scrollOffsetY = nfloat.Clamp(scrollOffsetY + (nfloat)e.Delta.Y * 30f, 0, MaxScrollOffset);
        flingCurve = null;
        pendingFlingVelocity = null;
        e.Handled = true;
        InvalidateArrange();
        InvalidateRender();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (pendingFlingVelocity.HasValue)
        {
            flingCurve = new ExponentialDecayCurve(
                startTime: current,
                startPosition: scrollOffsetY,
                initialVelocity: pendingFlingVelocity.Value,
                decayPerSecond: ExponentialDecayCurve.Normal);
            pendingFlingVelocity = null;
        }

        if (flingCurve is { } curve && !isDragging)
        {
            nfloat newOffset = curve[current];
            nfloat clamped = nfloat.Clamp(newOffset, 0, MaxScrollOffset);
            scrollOffsetY = clamped;

            bool atBoundary = clamped != newOffset;
            bool notDone = current < curve.EndTime;

            if (notDone && !atBoundary)
                RequestAnimationFrame();
            else
                flingCurve = null;

            InvalidateArrange();
            InvalidateRender();
        }
    }
}
