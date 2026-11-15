namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Describes the direction of a navigation transition.</summary>
public enum NavigationDirection
{
    /// <summary>No direction, used when a transition is not applicable.</summary>
    None,
    /// <summary>A push toward a newer page.</summary>
    Forward,
    /// <summary>A pop toward a previous page.</summary>
    Backward,
}
