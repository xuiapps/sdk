namespace Xui.DevKit.UI.Design;

/// <summary>
/// Root interface for the Xui Design System.
/// Resolved from the parent-chain service provider via GetService&lt;IDesignSystem&gt;().
/// </summary>
public interface IDesignSystem
{
    /// <summary>Color tokens and palette math.</summary>
    IColorSystem Colors { get; }

    /// <summary>Typography scale.</summary>
    ITypographySystem Typography { get; }

    /// <summary>Spacing tokens derived from the 4-pt grid.</summary>
    ISpacingSystem Spacing { get; }

    /// <summary>Shape / corner-radius tokens.</summary>
    IShapeSystem Shape { get; }

    /// <summary>Motion tokens (curves and springs).</summary>
    IMotionSystem Motion { get; }

    /// <summary>Raised with the affected token groups after an update; consumers invalidate only the required passes.</summary>
    event Action<DesignSystemChange>? Changed;
}

/// <summary>
/// Receives a design-token update after a view has resolved its <see cref="IDesignSystem"/>.
/// Implementations read shared tokens from the retained provider and invalidate only affected passes.
/// </summary>
public interface IDesignSystemChangeNotifications
{
    /// <summary>Responds to the changed token groups without duplicating theme state on the view.</summary>
    void OnDesignTokenChange(DesignSystemChange changes);
}
