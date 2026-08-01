using Xui.Core.UI;

namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Commands and state exposed by a navigation host.</summary>
public interface INavigation
{
    /// <summary>Gets whether the current page has a previous page.</summary>
    bool CanGoBack { get; }

    /// <summary>Gets whether a push, pop, or interactive pop is active.</summary>
    bool IsTransitioning { get; }

    /// <summary>Pushes <paramref name="page"/> above the current page.</summary>
    void Push(View page);

    /// <summary>Pops the current page, if one exists beneath it.</summary>
    void Pop();
}
