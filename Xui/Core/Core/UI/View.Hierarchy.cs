using System;

namespace Xui.Core.UI;

public partial class View
{
    /// <summary>
    /// Returns the number of child views. Used by layout containers and traversal logic.
    /// Leaf views should return 0.
    /// </summary>
    public virtual int Count { get; } = 0;

    /// <summary>
    /// Indexer to access child views by index.
    /// Layout containers should implement this to expose their children.
    /// </summary>
    public virtual View this[int index] { get => throw new IndexOutOfRangeException(); }

    /// <summary>
    /// Attaches a child view to this parent, setting Parent and performing basic validation.
    /// Intended for internal use by container views and single-child hosts.
    /// </summary>
    protected void AddProtectedChild(View child)
    {
        if (child is null) throw new ArgumentNullException(nameof(child));

        if (ReferenceEquals(child, this))
            throw new InvalidOperationException("A view cannot be its own child.");

        if (child.Parent is not null)
            throw new InvalidOperationException("View already has a parent.");

        child.Parent = this;

        if ((this.Flags & ViewFlags.Attached) != 0)
        {
            var attachEvent = new AttachEventRef();
            AttachSubtree(child, ref attachEvent);
        }

        if ((this.Flags & ViewFlags.Active) != 0)
            ActivateSubtree(child);

        // Attaching a child changes layout + visuals.
        this.InvalidateArrange();
        this.InvalidateRender();
    }

    /// <summary>
    /// Detaches a child view from this parent, clearing Parent.
    /// Intended for internal use by container views and single-child hosts.
    /// </summary>
    protected void RemoveProtectedChild(View child)
    {
        if (child is null) throw new ArgumentNullException(nameof(child));

        if (child.Parent != this)
            throw new InvalidOperationException("View is not a child of this parent.");

        if ((child.Flags & ViewFlags.Active) != 0)
            DeactivateSubtree(child);

        if ((child.Flags & ViewFlags.Attached) != 0)
        {
            var detachEvent = new DetachEventRef();
            DetachSubtree(child, ref detachEvent);
        }

        child.Parent = null;

        // Detaching a child changes layout + visuals.
        this.InvalidateArrange();
        this.InvalidateRender();
    }

    /// <summary>
    /// Sets a single protected child field, automatically detaching the old child (if any)
    /// and attaching the new child (if any). Use this for single-child containers.
    /// </summary>
    protected void SetProtectedChild<T>(ref T? field, T? value)
        where T : View
    {
        if (field == value)
            return;

        if (field is not null)
        {
            // Detach old
            this.RemoveProtectedChild(field);
            field = null;
        }

        if (value is not null)
        {
            // Attach new
            this.AddProtectedChild(value);
            field = value;
        }
    }
}