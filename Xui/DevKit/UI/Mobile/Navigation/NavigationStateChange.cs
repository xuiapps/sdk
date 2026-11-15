namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Allocation-free notification sent only to pages participating in a transition.</summary>
public readonly record struct NavigationStateChange(
    NavigationPhase Phase,
    NavigationDirection Direction,
    NavigationRole Role,
    nfloat Progress,
    bool IsInteractive);
