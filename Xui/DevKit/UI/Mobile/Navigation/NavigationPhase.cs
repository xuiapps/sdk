namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Describes the lifecycle point of a navigation transition.</summary>
public enum NavigationPhase
{
    /// <summary>The page is about to become visible.</summary>
    WillAppear,
    /// <summary>The page is becoming visible; use <see cref="NavigationStateChange.Progress"/>.</summary>
    Appearing,
    /// <summary>The page is fully visible.</summary>
    Appeared,
    /// <summary>The page is about to become hidden.</summary>
    WillDisappear,
    /// <summary>The page is becoming hidden; use <see cref="NavigationStateChange.Progress"/>.</summary>
    Disappearing,
    /// <summary>The page is no longer visible.</summary>
    Disappeared,
    /// <summary>An interactive transition was returned to its starting page.</summary>
    Cancelled,
}
