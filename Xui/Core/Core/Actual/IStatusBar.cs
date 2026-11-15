namespace Xui.Core.Actual;

/// <summary>
/// Optional window-scoped control of status-bar text and icons. Resolve through the
/// window or view service provider and use on the UI thread. Desktop runtimes may
/// leave this unregistered. This does not change the bar background or safe area.
/// </summary>
public interface IStatusBar
{
    /// <summary>Gets or sets the requested foreground appearance.</summary>
    StatusBarAppearance Appearance { get; set; }
}
