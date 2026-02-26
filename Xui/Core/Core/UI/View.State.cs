using Xui.Core.Actual;
using Xui.Core.Debug;

namespace Xui.Core.UI;

public partial class View
{
    /// <summary>
    /// Flags used internally by a <see cref="View"/> to track invalidation state across
    /// measure, arrangement, rendering, and hit testing phases.
    /// These flags allow the framework to selectively recompute only dirty subtrees
    /// during recursive passes.
    /// </summary>
    [Flags]
    public enum ViewFlags
    {
        /// <summary>
        /// This view is animated.
        /// It has properties that are calculated based on the current UI time.
        /// It requires an animation notification on the next frame.
        /// </summary>
        Animated = 1 << 0,

        /// <summary>
        /// This view has animated children.
        /// While this view directly does not depend on the next frame time,
        /// it must propagate an animation notification to its animated children.
        /// </summary>
        DescendantAnimated = 1 << 1,

        /// <summary>
        /// The <see cref="View"/> has changed since its last measure.
        /// It will likely return a different desired size if measured again
        /// with the same constraints.
        /// </summary>
        MeasureChanged = 1 << 2,

        /// <summary>
        /// The <see cref="View"/> has changed in a way that may cause it to arrange
        /// its children differently, even if the available rectangle and measure
        /// context are the same.
        /// </summary>
        ArrangeChanged = 1 << 3,

        /// <summary>
        /// One or more descendants of this <see cref="View"/> have had their
        /// <see cref="ArrangeChanged"/> flag set since the last validation.
        /// This flag does not indicate that this view itself needs re-arranging,
        /// but rather that some descendant may need to be re-arranged with its
        /// existing rectangle (e.g., a visual state change that affects only
        /// internal layout).
        /// </summary>
        DescendantArrangeChanged = 1 << 4,

        /// <summary>
        /// The <see cref="View"/> has changed visually since its last render pass,
        /// and <see cref="RenderCore"/> produce different output even if rendered within the same frame and context.
        /// </summary>
        RenderChanged = 1 << 5,

        /// <summary>
        /// One or more descendants of this <see cref="View"/> have had their
        /// <see cref="RenderChanged"/> flag set since the last validation.
        /// This is a hint that rendering work exists below, even if this view itself
        /// does not need re-rendering.
        /// </summary>
        DescendantRenderChanged = 1 << 6,

        /// <summary>
        /// This view's <see cref="HitTest"/> changed.
        /// It will probably return a new results for the same <see cref="Xui.Core.Math2D.Point"/>.
        /// </summary>
        HitTestChanged = 1 << 7,

        /// <summary>
        /// One or more descendants of this <see cref="View"/> have their hit testing changed.
        /// They will probably return a new result for the same <see cref="Xui.Core.Math2D.Point"/>.
        /// </summary>
        DescendantHitTestChanged = 1 << 8,

        /// <summary>
        /// This view is active — it is part of a live visual tree and will receive
        /// events, render, and animate. Set by <see cref="ActivateSubtree"/> and
        /// cleared by <see cref="DeactivateSubtree"/>.
        /// </summary>
        Active = 1 << 9,

        /// <summary>
        /// This view is attached — it has been added to a visual tree that is itself
        /// attached to the platform (e.g. a window). Set by <see cref="AttachSubtree"/>
        /// and cleared by <see cref="DetachSubtree"/>.
        /// </summary>
        Attached = 1 << 10,
    }

    /// <summary>
    /// Gets the current change flags for this view. These flags indicate which
    /// aspects (measure, arrange, render, hit test) have changed since the last
    /// time the parent acknowledged them via the corresponding Validate* method.
    /// </summary>
    public ViewFlags Flags { get; private set; }

    /// <summary>
    /// Clears per-frame animation flags so this view will not be considered animated
    /// unless <see cref="RequestAnimationFrame"/> is called during this pass.
    /// </summary>
    private void ResetAnimationFlags()
    {
        this.Flags &= ~(ViewFlags.Animated | ViewFlags.DescendantAnimated);
    }

    /// <summary>
    /// Requests another animation frame for this view on the next UI tick.
    /// Sets <see cref="ViewFlags.Animated"/> and notifies ancestors (via
    /// <see cref="OnChildRequestedAnimationFrame(View)"/>) so they carry
    /// <see cref="ViewFlags.DescendantAnimated"/>.
    /// </summary>
    protected void RequestAnimationFrame()
    {
        if ((this.Flags & ViewFlags.Animated) != 0)
            return;

        this.Flags |= ViewFlags.Animated;
        Runtime.CurrentInstruments.Log(Scope.ViewAnimation, LevelOfDetail.Info,
            $"RequestAnimationFrame {this.GetType().Name}");
        this.Parent?.OnChildRequestedAnimationFrame(this);
    }

    /// <summary>
    /// Called when a direct or indirect child has requested another animation frame
    /// for the next UI tick. The default implementation marks
    /// <see cref="ViewFlags.DescendantAnimated"/> and forwards the notification up
    /// the visual tree.
    /// </summary>
    /// <param name="child">The child view that requested the animation frame.</param>
    protected virtual void OnChildRequestedAnimationFrame(View child)
    {
        if ((this.Flags & ViewFlags.DescendantAnimated) != 0)
            return;

        this.Flags |= ViewFlags.DescendantAnimated;
        this.Parent?.OnChildRequestedAnimationFrame(this);
    }

    /// <summary>
    /// Marks this view as having changed in a way that may affect its measured size.
    /// Causes <see cref="OnChildMeasureChanged"/> to be invoked on the parent.
    /// </summary>
    protected void InvalidateMeasure()
    {
        if ((this.Flags & ViewFlags.MeasureChanged) != 0)
            return;

        this.Flags |= ViewFlags.MeasureChanged;
        this.Parent?.OnChildMeasureChanged(this);
    }

    /// <summary>
    /// Marks this view as having changed in a way that may cause it to arrange its
    /// children differently, even with the same rectangle and context.
    /// Causes <see cref="OnChildArrangeChanged"/> to be invoked on the parent.
    /// </summary>
    protected void InvalidateArrange()
    {
        if ((this.Flags & ViewFlags.ArrangeChanged) != 0)
            return;

        this.Flags |= ViewFlags.ArrangeChanged;
        this.Parent?.OnChildArrangeChanged(this);
    }

    /// <summary>
    /// Marks this view as having changed visually in a way that requires re-rendering.
    /// Causes <see cref="OnChildRenderChanged"/> to be invoked on the parent.
    /// </summary>
    protected void InvalidateRender()
    {
        if ((this.Flags & ViewFlags.RenderChanged) != 0)
            return;

        this.Flags |= ViewFlags.RenderChanged;
        Runtime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Info,
            $"InvalidateRender {this.GetType().Name} Flags={this.Flags}");
        this.Parent?.OnChildRenderChanged(this);
    }

    /// <summary>
    /// Marks this view as having changed in a way that may cause hit testing to yield
    /// different results, even for the same input coordinates.
    /// Causes <see cref="OnChildHitTestChanged"/> to be invoked on the parent.
    /// </summary>
    protected void InvalidateHitTest()
    {
        if ((this.Flags & ViewFlags.HitTestChanged) != 0)
            return;

        this.Flags |= ViewFlags.HitTestChanged;
        this.Parent?.OnChildHitTestChanged(this);
    }

    /// <summary>
    /// Clears the <see cref="ViewFlags.MeasureChanged"/> flag on this view, indicating that
    /// a parent or container has acknowledged the child’s measure change and handled it
    /// (e.g., decided to keep the same column width in a grid regardless of the child’s new desired size).
    /// Use this when the parent has considered the change and no re-measure is required for this child.
    /// </summary>
    public void ValidateMeasure()
    {
        this.Flags &= ~ViewFlags.MeasureChanged;
    }

    /// <summary>
    /// Clears the <see cref="ViewFlags.ArrangeChanged"/> flag on this view, indicating that
    /// a parent or container has acknowledged the child’s arrange change.
    /// </summary>
    public void ValidateArrange()
    {
        this.Flags &= ~ViewFlags.ArrangeChanged;
        this.Flags &= ~ViewFlags.DescendantArrangeChanged;
    }

    /// <summary>
    /// Clears the <see cref="ViewFlags.RenderChanged"/> flag on this view, indicating that
    /// the parent (or the rendering pipeline) has acknowledged the visual change and
    /// re-rendering for this view has been completed.
    /// </summary>
    public void ValidateRender()
    {
        if ((this.Flags & (ViewFlags.RenderChanged | ViewFlags.DescendantRenderChanged)) != 0)
        {
            Runtime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Diagnostic,
                $"ValidateRender {this.GetType().Name} clearing Flags={this.Flags & (ViewFlags.RenderChanged | ViewFlags.DescendantRenderChanged)}");
        }
        this.Flags &= ~ViewFlags.RenderChanged;
        this.Flags &= ~ViewFlags.DescendantRenderChanged;
    }

    /// <summary>
    /// Clears the <see cref="ViewFlags.HitTestChanged"/> flag on this view, indicating that
    /// hit-test caches (if any) have been updated or the parent has otherwise accounted for
    /// the changed hit-test behavior.
    /// </summary>
    public void ValidateHitTest()
    {
        this.Flags &= ~ViewFlags.HitTestChanged;
        this.Flags &= ~ViewFlags.DescendantHitTestChanged;
    }

    /// <summary>
    /// Called when a direct or indirect child view has changed in a way that may
    /// affect its measured size. The default implementation forwards the notification
    /// up the visual tree to the parent.
    /// </summary>
    /// <param name="child">The child view whose measure state has changed.</param>
    protected virtual void OnChildMeasureChanged(View child)
    {
        this.InvalidateArrange();
    }

    /// <summary>
    /// Called when a direct or indirect child view has changed in a way that may
    /// affect its arrangement of children. The default implementation forwards the
    /// notification up the visual tree to the parent.
    /// </summary>
    /// <param name="child">The child view whose arrange state has changed.</param>
    protected virtual void OnChildArrangeChanged(View child)
    {
        if ((this.Flags & ViewFlags.DescendantArrangeChanged) != 0)
            return;

        this.Flags |= ViewFlags.DescendantArrangeChanged;
        this.Parent?.OnChildArrangeChanged(this);
    }

    /// <summary>
    /// Called when a direct or indirect child view has changed visually and requires
    /// re-rendering. The default implementation forwards the notification up the visual tree.
    /// </summary>
    /// <param name="child">The child view whose render state has changed.</param>
    protected virtual void OnChildRenderChanged(View child)
    {
        if ((this.Flags & ViewFlags.DescendantRenderChanged) != 0)
        {
            Runtime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Info,
                $"OnChildRenderChanged {this.GetType().Name} <- {child.GetType().Name} (propagation stopped, DescendantRenderChanged already set)");
            return;
        }

        this.Flags |= ViewFlags.DescendantRenderChanged;
        Runtime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Info,
            $"OnChildRenderChanged {this.GetType().Name} <- {child.GetType().Name}");
        this.Parent?.OnChildRenderChanged(this);
    }

    /// <summary>
    /// Called when a direct or indirect child view has changed in a way that may cause
    /// hit testing results to change. The default implementation forwards the notification
    /// up the visual tree to the parent.
    /// </summary>
    /// <param name="child">The child view whose hit testing state has changed.</param>
    protected virtual void OnChildHitTestChanged(View child)
    {
        if ((this.Flags & ViewFlags.DescendantHitTestChanged) != 0)
            return;

        this.Flags |= ViewFlags.DescendantHitTestChanged;
        this.Parent?.OnChildHitTestChanged(this);
    }

    /// <summary>
    /// Captures the pointer with the specified identifier so that it continues to
    /// receive pointer events even if the pointer moves outside its bounds.
    /// </summary>
    /// <param name="pointerId">The platform-assigned pointer identifier.</param>
    public void CapturePointer(int pointerId)
    {
        if (this.TryFindParent<RootView>(out var rootView))
        {
            rootView.EventRouter.CapturePointer(this, pointerId);
        }
    }

    /// <summary>
    /// Releases a previously captured pointer so that pointer events are routed
    /// according to normal hit testing rules.
    /// </summary>
    /// <param name="pointerId">The platform-assigned pointer identifier.</param>
    public void ReleasePointer(int pointerId)
    {
        if (this.TryFindParent<RootView>(out var rootView))
        {
            rootView.EventRouter.ReleasePointer(this, pointerId);
        }
    }
}
