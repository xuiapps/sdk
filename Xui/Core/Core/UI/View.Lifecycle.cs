namespace Xui.Core.UI;

public partial class View
{
    /// <summary>
    /// Called when this view is added to the visual tree and platform contexts become available.
    /// Override to acquire resources that depend on text measurement or bitmap factories.
    /// </summary>
    protected virtual void OnAttach(ref AttachEventRef e) { }

    /// <summary>
    /// Called when this view is removed from the visual tree.
    /// Override to release any resources acquired in <see cref="OnAttach"/>.
    /// </summary>
    protected virtual void OnDetach(ref DetachEventRef e) { }

    /// <summary>
    /// Called when this view becomes active — it will receive events, render, and animate.
    /// Override this to start animations, subscribe to data sources, or acquire resources.
    /// </summary>
    protected virtual void OnActivate() { }

    /// <summary>
    /// Called when this view becomes dormant — it should stop animations, timers, and event subscriptions.
    /// The view may remain in the tree (e.g., in a virtualizing panel's recycle pool).
    /// </summary>
    protected virtual void OnDeactivate() { }

    /// <summary>
    /// Walks the subtree top-down, calling <see cref="OnAttach"/> on each view.
    /// Parent attaches before children.
    /// </summary>
    internal static void AttachSubtree(View view, ref AttachEventRef e)
    {
        view.Flags |= ViewFlags.Attached;
        view.OnAttach(ref e);

        for (int i = 0; i < view.Count; i++)
        {
            AttachSubtree(view[i], ref e);
        }
    }

    /// <summary>
    /// Walks the subtree bottom-up, calling <see cref="OnDetach"/> on each view.
    /// Children detach before parent.
    /// </summary>
    internal static void DetachSubtree(View view, ref DetachEventRef e)
    {
        for (int i = 0; i < view.Count; i++)
        {
            DetachSubtree(view[i], ref e);
        }

        view.OnDetach(ref e);
        view.Flags &= ~ViewFlags.Attached;
    }

    /// <summary>
    /// Walks the subtree top-down, setting <see cref="ViewFlags.Active"/> and calling
    /// <see cref="OnActivate"/> on each view. Parent activates before children.
    /// </summary>
    internal static void ActivateSubtree(View view)
    {
        view.Flags |= ViewFlags.Active;
        view.OnActivate();

        for (int i = 0; i < view.Count; i++)
        {
            ActivateSubtree(view[i]);
        }
    }

    /// <summary>
    /// Walks the subtree bottom-up, calling <see cref="OnDeactivate"/> and clearing
    /// <see cref="ViewFlags.Active"/> on each view. Children deactivate before parent.
    /// </summary>
    internal static void DeactivateSubtree(View view)
    {
        for (int i = 0; i < view.Count; i++)
        {
            DeactivateSubtree(view[i]);
        }

        view.OnDeactivate();
        view.Flags &= ~ViewFlags.Active;
    }
}
