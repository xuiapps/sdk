using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>
/// A stack-based navigation host with UIKit-style page transitions and an interactive leading-edge pop gesture.
/// It owns page surfaces, clipping, transition shading, and lifecycle notifications.
/// </summary>
public class NavigationView : ViewCollection, INavigation, IFullScreenView
{
    private readonly List<View> stack = [];
    private readonly PageContent backPageContent;
    private readonly PageContent frontPageContent;
    private View currentPage;
    private View? outgoingPage;
    private TimeSpan? transitionStart;
    private TimeSpan transitionDuration;
    private bool isForwardTransition;
    private bool isInteractiveTransition;
    private bool transitionWasInteractive;
    private nfloat transitionProgress;
    private nfloat transitionStartProgress;
    private nfloat transitionTargetProgress;
    private int edgeSwipePointerId = -1;
    private nfloat edgeSwipeStartX;
    private nfloat edgeSwipeStartY;
    private nfloat edgeSwipeLastX;
    private long edgeSwipeLastTick;
    private nfloat edgeSwipeVelocity;
    private bool rootPageAppeared;
    private View? overlayContent;
    private Color defaultPageBackground = Colors.White;

    /// <summary>Duration of non-interactive pushes and pops.</summary>
    // Deliberately slowed for transition profiling; restore 350 ms after the investigation.
    public TimeSpan TransitionDuration { get; set; } = TimeSpan.FromMilliseconds(350);

    /// <summary>Fast decelerating curve used by UIKit-style navigation transitions.</summary>
    public Easing.CubicBezier TransitionEasing { get; set; } = new((0.32f, 0.72f), (0f, 1f));

    /// <summary>Width of the leading-edge region that may start an interactive pop.</summary>
    public nfloat BackSwipeEdgeWidth { get; set; } = 25;

    /// <summary>Default opaque surface behind pages that do not provide a background.</summary>
    public Color DefaultPageBackground
    {
        get => defaultPageBackground;
        set => SetViewRenderProperty(ref defaultPageBackground, value);
    }

    /// <inheritdoc/>
    public bool DrawsOpaqueFullScreen => GetBackgroundColor().Alpha == 1;

    private Color GetBackgroundColor() =>
        GetPresentation(backPageContent.Content ?? currentPage).Background ?? DefaultPageBackground;

    /// <summary>Gets or sets whether the back page is darkened during navigation transitions.</summary>
    public bool UsesBackgroundTransitionShade { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the foreground page receives its rounded gradient edge shadow.
    /// Disabled while isolating the cost of the gradient shadow from the flat shade.
    /// </summary>
    public bool UsesRoundedPageTransitionShadow { get; set; } = true;

    /// <summary>Optional content rendered above the page stack and docked at the navigation viewport's top-right corner.</summary>
    public View? OverlayContent
    {
        get => overlayContent;
        set
        {
            if (ReferenceEquals(overlayContent, value))
                return;
            if (overlayContent is not null)
                Remove(overlayContent);
            overlayContent = value;
            if (overlayContent is not null)
                Add(overlayContent);
        }
    }

    /// <summary>Creates a navigation stack with <paramref name="rootPage"/> as its first page.</summary>
    public NavigationView(View rootPage)
    {
        ArgumentNullException.ThrowIfNull(rootPage);
        stack.Add(rootPage);
        currentPage = rootPage;
        backPageContent = new PageContent(this);
        frontPageContent = new PageContent(this) { Content = rootPage, ClipToPresentation = false };
        Add(backPageContent);
        Add(frontPageContent);
    }

    /// <inheritdoc/>
    public bool CanGoBack => stack.Count > 1;

    /// <inheritdoc/>
    public bool IsTransitioning => outgoingPage is not null;

    /// <inheritdoc/>
    public void Push(View page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (outgoingPage is not null)
            return;

        stack.Add(page);
        StartTransition(page, isForward: true);
    }

    /// <inheritdoc/>
    public void Pop()
    {
        if (outgoingPage is not null || !CanGoBack)
            return;

        StartTransition(stack[^2], isForward: false);
    }

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        base.OnActivate();
        if (rootPageAppeared)
            return;

        rootPageAppeared = true;
        Notify(currentPage, NavigationPhase.Appeared, NavigationDirection.None, NavigationRole.Incoming, 1, false);
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var bounds = GetPageBounds(available);
        return currentPage.Measure(bounds, context);
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        var bounds = GetPageBounds(rect);
        backPageContent.Arrange(bounds, context);
        frontPageContent.Arrange(bounds, context);
        if (overlayContent is not null)
        {
            var overlaySize = overlayContent.Measure(bounds.Size, context);
            overlayContent.Arrange(new Rect(bounds.Right - overlaySize.Width - 12, bounds.Y + 12, overlaySize.Width, overlaySize.Height), context);
        }
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        // One stationary background covers the display, including safe-area insets.
        // Only the moving foreground page needs an additional surface and rounded clip.
        context.Save();
        try
        {
            ClipNavigationViewport(context);
            context.Save();
            try
            {
                if (IsTransitioning && UsesBackgroundTransitionShade)
                    context.SetFilter(new Brightness(GetPreviousPageBrightness()));
                context.SetFill(GetBackgroundColor());
                context.FillRect(GetDisplayBounds());
                if (outgoingPage is not null)
                    RenderBackPage(context);
            }
            finally
            {
                context.Restore();
            }
            if (outgoingPage is null)
            {
                this.RenderChild(context, frontPageContent);
                RenderOverlay(context);
                return;
            }

            if (isForwardTransition)
            {
                DrawPageShadow(context, currentPage, frontPageContent.Transform.Tx);
                this.RenderChild(context, frontPageContent);
                RenderOverlay(context);
                return;
            }

            DrawPageShadow(context, outgoingPage, frontPageContent.Transform.Tx);
            this.RenderChild(context, frontPageContent);
            RenderOverlay(context);
        }
        finally
        {
            context.Restore();
        }
    }

    private void RenderOverlay(IContext context)
    {
        if (overlayContent is not null)
            this.RenderChild(context, overlayContent);
    }

    private void RenderBackPage(IContext context)
    {
        var exposedBounds = GetExposedBackPageBounds();
        if (exposedBounds.Width <= 0 || exposedBounds.Height <= 0)
            return;

        context.Save();
        try
        {
            // The foreground page is opaque. Restricting both canvas clipping and
            // traversal culling to its exposed leading edge avoids repainting page
            // content that will be covered by the foreground surface.
            context.BeginPath();
            context.Rect(exposedBounds);
            context.Clip();
            this.RenderChild(context, backPageContent, exposedBounds);
        }
        finally
        {
            context.Restore();
        }
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble)
            return;

        if (e.Type == PointerEventType.Down && e.State.PointerType == PointerType.Touch &&
            outgoingPage is null && CanGoBack && e.State.Position.X <= BackSwipeEdgeWidth)
        {
            edgeSwipePointerId = e.PointerId;
            edgeSwipeStartX = e.State.Position.X;
            edgeSwipeStartY = e.State.Position.Y;
            edgeSwipeLastX = edgeSwipeStartX;
            edgeSwipeLastTick = Environment.TickCount64;
            edgeSwipeVelocity = 0;
            return;
        }

        if (e.PointerId != edgeSwipePointerId)
            return;

        if (!isInteractiveTransition)
        {
            if (e.Type == PointerEventType.Move)
            {
                var horizontal = e.State.Position.X - edgeSwipeStartX;
                var vertical = e.State.Position.Y - edgeSwipeStartY;
                if (nfloat.Abs(vertical) > 8 && nfloat.Abs(vertical) > nfloat.Abs(horizontal))
                    edgeSwipePointerId = -1;
                else if (horizontal > 8 && horizontal > nfloat.Abs(vertical))
                {
                    StartInteractivePop(ref e);
                    UpdateInteractivePop(e.State.Position.X);
                }
            }
            else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
            {
                edgeSwipePointerId = -1;
            }

            return;
        }

        switch (e.Type)
        {
            case PointerEventType.Move: UpdateInteractivePop(e.State.Position.X); break;
            case PointerEventType.Up: EndInteractivePop(ref e, cancelled: false); break;
            case PointerEventType.Cancel: EndInteractivePop(ref e, cancelled: true); break;
            case PointerEventType.LostCapture:
                edgeSwipePointerId = -1;
                SettleInteractivePop(complete: false);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (outgoingPage is null || isInteractiveTransition)
            return;

        transitionStart ??= current;
        var linearProgress = nfloat.Clamp((nfloat)((current - transitionStart.Value) / transitionDuration), 0, 1);
        var easedProgress = TransitionEasing.Evaluate(linearProgress, 0.0005f);
        transitionProgress = transitionStartProgress + (transitionTargetProgress - transitionStartProgress) * easedProgress;
        UpdatePageTransforms();
        NotifyProgress();
        InvalidateRender();

        if (linearProgress < 1)
        {
            RequestAnimationFrame();
            return;
        }

        CompleteTransition();
    }

    private void StartTransition(View incomingPage, bool isForward)
    {
        outgoingPage = currentPage;
        currentPage = incomingPage;
        isForwardTransition = isForward;
        isInteractiveTransition = false;
        transitionWasInteractive = false;
        transitionProgress = transitionStartProgress = 0;
        transitionTargetProgress = 1;
        transitionDuration = TransitionDuration;
        transitionStart = null;
        if (isForward)
        {
            var previousPage = frontPageContent.Content;
            frontPageContent.Content = null;
            backPageContent.Content = previousPage;
            frontPageContent.Content = incomingPage;
        }
        else
        {
            backPageContent.Content = incomingPage;
        }
        UpdatePageTransforms();
        Notify(outgoingPage, NavigationPhase.WillDisappear, isForward ? NavigationDirection.Forward : NavigationDirection.Backward, NavigationRole.Outgoing, 0, false);
        Notify(currentPage, NavigationPhase.WillAppear, isForward ? NavigationDirection.Forward : NavigationDirection.Backward, NavigationRole.Incoming, 0, false);
        RequestAnimationFrame();
        InvalidateRender();
    }

    private void StartInteractivePop(ref PointerEventRef e)
    {
        outgoingPage = currentPage;
        currentPage = stack[^2];
        isForwardTransition = false;
        isInteractiveTransition = true;
        transitionWasInteractive = true;
        transitionProgress = transitionStartProgress = 0;
        transitionTargetProgress = 1;
        transitionStart = null;
        backPageContent.Content = currentPage;
        UpdatePageTransforms();
        Notify(outgoingPage, NavigationPhase.WillDisappear, NavigationDirection.Backward, NavigationRole.Outgoing, 0, true);
        Notify(currentPage, NavigationPhase.WillAppear, NavigationDirection.Backward, NavigationRole.Incoming, 0, true);
        CapturePointer(e.PointerId, PointerGestures.Drag);
        InvalidateRender();
    }

    private void UpdateInteractivePop(nfloat x)
    {
        transitionProgress = nfloat.Clamp((x - edgeSwipeStartX) / nfloat.Max(Frame.Width, 1), 0, 1);
        var now = Environment.TickCount64;
        var elapsed = now - edgeSwipeLastTick;
        if (elapsed > 0)
            edgeSwipeVelocity = (x - edgeSwipeLastX) * 1000 / elapsed;
        edgeSwipeLastX = x;
        edgeSwipeLastTick = now;
        UpdatePageTransforms();
        NotifyProgress();
        InvalidateRender();
    }

    private void EndInteractivePop(ref PointerEventRef e, bool cancelled)
    {
        var complete = !cancelled && (transitionProgress >= 0.5f || edgeSwipeVelocity >= 800);
        edgeSwipePointerId = -1;
        ReleasePointer(e.PointerId);
        SettleInteractivePop(complete);
    }

    private void SettleInteractivePop(bool complete)
    {
        isInteractiveTransition = false;
        transitionStartProgress = transitionProgress;
        transitionTargetProgress = complete ? 1 : 0;
        transitionDuration = TimeSpan.FromMilliseconds(nfloat.Clamp(
            nfloat.Abs(transitionTargetProgress - transitionStartProgress) * (nfloat)TransitionDuration.TotalMilliseconds,
            120,
            (nfloat)TransitionDuration.TotalMilliseconds));
        transitionStart = null;
        UpdatePageTransforms();
        RequestAnimationFrame();
        InvalidateRender();
    }

    private void CompleteTransition()
    {
        if (outgoingPage is null)
            return;

        var direction = isForwardTransition ? NavigationDirection.Forward : NavigationDirection.Backward;
        if (transitionTargetProgress == 0)
        {
            Notify(outgoingPage, NavigationPhase.Cancelled, direction, NavigationRole.Outgoing, transitionProgress, transitionWasInteractive);
            Notify(currentPage, NavigationPhase.Cancelled, direction, NavigationRole.Incoming, transitionProgress, transitionWasInteractive);
            backPageContent.Content = null;
            currentPage = outgoingPage;
            Notify(currentPage, NavigationPhase.Appeared, direction, NavigationRole.Outgoing, 1, transitionWasInteractive);
            Notify(stack[^2], NavigationPhase.Disappeared, direction, NavigationRole.Incoming, 1, transitionWasInteractive);
        }
        else
        {
            if (isForwardTransition)
            {
                backPageContent.Content = null;
            }
            else
            {
                var revealedPage = backPageContent.Content;
                frontPageContent.Content = null;
                backPageContent.Content = null;
                frontPageContent.Content = revealedPage;
            }
            if (!isForwardTransition)
                stack.RemoveAt(stack.Count - 1);
            Notify(outgoingPage, NavigationPhase.Disappeared, direction, NavigationRole.Outgoing, 1, transitionWasInteractive);
            Notify(currentPage, NavigationPhase.Appeared, direction, NavigationRole.Incoming, 1, transitionWasInteractive);
        }

        outgoingPage = null;
        transitionStart = null;
        transitionProgress = transitionTargetProgress;
        UpdatePageTransforms();
    }

    private void NotifyProgress()
    {
        if (outgoingPage is null)
            return;
        var direction = isForwardTransition ? NavigationDirection.Forward : NavigationDirection.Backward;
        Notify(outgoingPage, NavigationPhase.Disappearing, direction, NavigationRole.Outgoing, transitionProgress, isInteractiveTransition);
        Notify(currentPage, NavigationPhase.Appearing, direction, NavigationRole.Incoming, transitionProgress, isInteractiveTransition);
    }

    private static void Notify(View page, NavigationPhase phase, NavigationDirection direction, NavigationRole role, nfloat progress, bool interactive)
    {
        if (page is INavigationAware aware)
        {
            var change = new NavigationStateChange(phase, direction, role, progress, interactive);
            aware.OnNavigationStateChanged(in change);
        }
    }

    private void UpdatePageTransforms()
    {
        if (outgoingPage is null)
        {
            backPageContent.Transform = AffineTransform.Identity;
            frontPageContent.Transform = AffineTransform.Identity;
            backPageContent.ClipToPresentation = false;
            frontPageContent.ClipToPresentation = false;
            return;
        }

        var distance = Frame.Width;
        var outgoingOffset = isForwardTransition ? -transitionProgress * distance * 0.3f : transitionProgress * distance;
        var incomingOffset = isForwardTransition
            ? (1 - transitionProgress) * distance
            : -(1 - transitionProgress) * distance * 0.3f;
        if (isForwardTransition)
        {
            // Push: the old page lives in the back host and the new page is frontmost.
            backPageContent.Transform = AffineTransform.Translate((outgoingOffset, 0));
            frontPageContent.Transform = AffineTransform.Translate((incomingOffset, 0));
        }
        else
        {
            // Pop: the revealed page lives in the back host while the current,
            // outgoing page remains frontmost and follows the user's finger.
            backPageContent.Transform = AffineTransform.Translate((incomingOffset, 0));
            frontPageContent.Transform = AffineTransform.Translate((outgoingOffset, 0));
        }
        backPageContent.ClipToPresentation = false;
        frontPageContent.ClipToPresentation = true;
    }

    private void ClipPage(IContext context, PageClipMode clip)
    {
        if (clip == PageClipMode.None)
            return;
        context.BeginPath();
        if (clip == PageClipMode.Device)
            context.RoundRect(GetDisplayBounds(), GetScreenCornerRadius());
        else
            context.Rect(Frame);
        context.Clip();
    }

    private void ClipNavigationViewport(IContext context)
    {
        context.BeginPath();
        context.Rect(GetDisplayBounds());
        context.Clip();
    }

    private nfloat GetPreviousPageBrightness() => isForwardTransition
        ? 1 - transitionProgress * 0.2f
        : 0.5f + transitionProgress * 0.5f;

    private void DrawPageShadow(IContext context, View topPage, nfloat xOffset)
    {
        if (!UsesRoundedPageTransitionShadow)
            return;

        var presentation = GetPresentation(topPage);
        // Preserve the edge definition for nearly the whole motion. It fades only as
        // the page settles, so an interactive pop retains its depth until completion.
        nfloat opacity = 0.08f * GetPageShadowVisibility(transitionProgress);
        nfloat extent = 45f;
        if (!presentation.UsesTransitionShadow || opacity <= 0 || extent <= 0)
            return;

        var page = GetPresentationBounds(presentation);
        page = new Rect(page.X + xOffset, page.Y, page.Width, page.Height);
        var radius = presentation.Clip == PageClipMode.Device ? GetScreenCornerRadius() : 0;
        radius = nfloat.Clamp(radius, 0, nfloat.Min(page.Width, page.Height) / 2);
        var edgeColor = new Color(0, 0, 0, opacity);
        var transparent = new Color(0, 0, 0, 0);

        context.Save();
        try
        {
            // Only the leading edge can be exposed inside the navigation viewport.
            // The other patches are hidden by the foreground page or viewport clip.
            var verticalLength = nfloat.Max(0, page.Height - radius * 2);
            DrawShadowStrip(context, new Rect(page.X - extent, page.Y + radius, extent, verticalLength),
                (page.X, page.Y), (page.X - extent, page.Y), edgeColor, transparent);

            if (radius > 0)
            {
                // Continue the leading-edge falloff around its two visible corners.
                DrawCornerShadow(context, new Point(page.X + radius, page.Y + radius), radius, extent, edgeColor, transparent, -1, -1);
                DrawCornerShadow(context, new Point(page.X + radius, page.Bottom - radius), radius, extent, edgeColor, transparent, -1, 1);
            }
        }
        finally { context.Restore(); }
    }

    private static nfloat GetPageShadowVisibility(nfloat progress)
    {
        progress = nfloat.Clamp(progress, 0, 1);
        return 1 - Easing.EaseInQuad(progress);
    }

    private static void DrawShadowStrip(IContext context, Rect rect, Point edge, Point outer, Color edgeColor, Color transparent)
    {
        context.SetFill(new LinearGradient(edge, outer, [new(0, edgeColor), new(1, transparent)]));
        context.FillRect(rect);
    }

    private static void DrawCornerShadow(
        IContext context,
        Point center,
        nfloat radius,
        nfloat extent,
        Color edgeColor,
        Color transparent,
        nfloat horizontalDirection,
        nfloat verticalDirection)
    {
        var outerRadius = radius + extent;
        var patch = new Rect(
            horizontalDirection < 0 ? center.X - outerRadius : center.X,
            verticalDirection < 0 ? center.Y - outerRadius : center.Y,
            outerRadius,
            outerRadius);
        var edgeStop = radius / outerRadius;
        context.SetFill(new RadialGradient(center, outerRadius,
            [new(0, edgeColor), new(edgeStop, edgeColor), new(1, transparent)]));
        context.FillRect(patch);
    }

    private PagePresentation GetPresentation(View page) => page is IPage pageInfo
        ? pageInfo.Presentation
        : new();

    private Rect GetPresentationBounds(PagePresentation presentation) => presentation.Clip == PageClipMode.Device
        ? GetDisplayBounds()
        : Frame;

    private Rect GetExposedBackPageBounds()
    {
        var viewport = GetDisplayBounds();
        var topPage = frontPageContent.Content;
        if (topPage is null)
            return viewport;

        var presentation = GetPresentation(topPage);
        var topPageBounds = GetPresentationBounds(presentation);
        var radius = presentation.Clip == PageClipMode.Device ? GetScreenCornerRadius() : 0;
        var right = nfloat.Clamp(
            topPageBounds.X + frontPageContent.Transform.Tx + radius,
            viewport.Left,
            viewport.Right);
        return new Rect(viewport.Left, viewport.Top, right - viewport.Left, viewport.Height);
    }

    private Size GetPageBounds(Size fallback) => GetPageBounds(new Rect(Frame.TopLeft, fallback)).Size;

    private Rect GetPageBounds(Rect fallback)
    {
        if (this.TryFindParent<RootView>(out var rootView))
            return rootView.Window.SafeArea;
        return fallback;
    }

    private Rect GetDisplayBounds() => this.TryFindParent<RootView>(out var rootView) ? rootView.Window.DisplayArea : Frame;

    private nfloat GetScreenCornerRadius() => this.TryFindParent<RootView>(out var rootView) ? rootView.Window.ScreenCornerRadius : 0;

    /// <summary>
    /// A page surface whose layout stays at its natural page bounds while its
    /// presentation transform controls slide position, hit testing, and culling.
    /// </summary>
    private sealed class PageContent : TransformView
    {
        private readonly NavigationView owner;

        public PageContent(NavigationView owner) => this.owner = owner;

        /// <summary>Gets or sets whether this page applies its presentation clip.</summary>
        public bool ClipToPresentation { get; set; }

        /// <inheritdoc/>
        protected override void RenderTransformedContent(IContext context)
        {
            var page = this.Content;
            if (page is null)
                return;

            var presentation = owner.GetPresentation(page);
            if (ClipToPresentation)
                owner.ClipPage(context, presentation.Clip);
            if (owner.IsTransitioning && ReferenceEquals(this, owner.frontPageContent))
            {
                context.SetFill(presentation.Background ?? owner.DefaultPageBackground);
                context.FillRect(owner.GetDisplayBounds());
            }
            base.RenderTransformedContent(context);
        }
    }
}
