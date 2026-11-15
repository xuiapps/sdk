namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Optional page metadata used by <see cref="NavigationView"/>.</summary>
public interface IPage
{
    /// <summary>Gets the page's surface, clipping, and transition-shadow preferences.</summary>
    PagePresentation Presentation { get; }
}
