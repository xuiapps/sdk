using Xui.Core.UI;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>Caches a view's design-system provider and routes token change notifications.</summary>
internal sealed class DesignSystemCache
{
    private bool resolved;
    private IDesignSystem? designSystem;

    /// <summary>Gets the cached provider, or <c>null</c> when the view has no design-system ancestor.</summary>
    public IDesignSystem? Current => designSystem;

    /// <summary>
    /// Resolves the provider once and notifies <paramref name="listener"/> of the initial theme.
    /// Later changes are delivered directly by <see cref="IDesignSystem.Changed"/>.
    /// </summary>
    public void Resolve(View view, IDesignSystemChangeNotifications listener)
    {
        if (resolved) return;

        designSystem = view.GetService(typeof(IDesignSystem)) as IDesignSystem;
        resolved = true;
        if (designSystem is null) return;

        designSystem.Changed += listener.OnDesignTokenChange;
        listener.OnDesignTokenChange(DesignSystemChange.All);
    }

    /// <summary>Stops observing the provider while the owning view is inactive.</summary>
    public void Deactivate(IDesignSystemChangeNotifications listener)
    {
        if (designSystem is not null)
            designSystem.Changed -= listener.OnDesignTokenChange;

        designSystem = null;
        resolved = false;
    }
}
