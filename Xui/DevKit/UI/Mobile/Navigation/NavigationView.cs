using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
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
    private readonly object viewportKey = new();
    private readonly object shadeKey = new();

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        using var viewport = context.Group(new GroupOptions
        {
            Key = viewportKey, ViewBacking = true, Bounds = GetDisplayBounds(),
            ClipToBounds = true, Background = GetBackgroundColor()
        });
        if (!viewport.NeedsRedraw) return;
        if (outgoingPage is not null)
        {
            // Retain the complete back-page surface; the foreground covers its hidden portion.
            this.RenderChild(context, backPageContent);
            if (IsTransitioning && UsesBackgroundTransitionShade)
            {
                using var shade = context.Group(new GroupOptions
                {
                    Key = shadeKey, ViewBacking = true, Bounds = GetDisplayBounds(), HasUpdates = false,
                    Background = new Color(0, 0, 0, 1 - GetPreviousPageBrightness())
                });
            }
        }
        this.RenderChild(context, frontPageContent);
        RenderOverlay(context);
    }

    private void RenderOverlay(IContext context)
    {
        if (overlayContent is not null)
        {
            using var overlay = context.Group(new GroupOptions { Key = overlayContent, ViewBacking = true, Bounds = overlayContent.Frame });
            if (overlay.NeedsRedraw) this.RenderChild(context, overlayContent);
        }
    }



    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down && e.State.PointerType == PointerType.Touch &&
            outgoingPage is null && CanGoBack && e.State.Position.X <= BackSwipeEdgeWidth)
            e.PushPotentialGesture(this, PointerGestures.DragHorizontalTentative);
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





    private nfloat GetPreviousPageBrightness() => isForwardTransition
        ? 1 - transitionProgress * 0.2f
        : 0.5f + transitionProgress * 0.5f;









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
        protected override void RenderCore(IContext context)
        {
            var page = Content;
            if (page is null) return;
            var presentation = owner.GetPresentation(page);
            var front = ReferenceEquals(this, owner.frontPageContent);
            var radius = ClipToPresentation && presentation.Clip == PageClipMode.Device ? owner.GetScreenCornerRadius() : 0;
            OutlineShadow? shadow = null;
            if (front && owner.IsTransitioning && owner.UsesRoundedPageTransitionShadow && presentation.UsesTransitionShadow)
                shadow = new OutlineShadow(new Color(0, 0, 0, (nfloat).12), 16, (-4, 0));
            using var group = context.Group(new GroupOptions
            {
                Key = page, ViewBacking = true, Bounds = owner.GetDisplayBounds(), Transform = Transform,
                HasUpdates = page.RequiresRender(context),
                ClipToBounds = ClipToPresentation, CornerRadius = radius, Shadow = shadow,
                Background = owner.IsTransitioning && front ? presentation.Background ?? owner.DefaultPageBackground : null
            });
            // Record the whole surface: ancestor visibility changes as this cached page moves.
            // Native bounds clipping handles exposure without baking a partial page into its cache.
            if (group.NeedsRedraw) RenderChild(context, page, owner.GetDisplayBounds());
        }

    }
}
