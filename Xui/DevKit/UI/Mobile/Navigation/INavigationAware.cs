namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Implemented by pages that synchronize custom visuals with navigation progress.</summary>
public interface INavigationAware
{
    /// <summary>Receives lifecycle and progress changes for this page.</summary>
    void OnNavigationStateChanged(in NavigationStateChange change);
}
