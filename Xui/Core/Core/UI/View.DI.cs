namespace Xui.Core.UI;

/// <summary>
/// Base class for all UI elements in the Xui layout engine.
/// A view participates in layout, rendering, and input hit testing, and may contain child views.
/// </summary>
public partial class View : IServiceProvider
{
    public virtual object? GetService(Type serviceType) =>
        this.Parent?.GetService(serviceType);
}
