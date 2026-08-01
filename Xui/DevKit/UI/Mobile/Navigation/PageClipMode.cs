namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Controls the clipping applied by <see cref="NavigationView"/> to a page.</summary>
public enum PageClipMode
{
    /// <summary>Clips to the display bounds and physical screen corner radius.</summary>
    Device,
    /// <summary>Clips to the square navigation bounds.</summary>
    Bounds,
    /// <summary>Does not apply a navigation clip.</summary>
    None,
}
