namespace Xui.Core.UI;

/// <summary>
/// Service interface for programmatic focus navigation.
/// Resolved via <c>this.GetService&lt;IFocus&gt;()</c> from any <see cref="View"/>.
/// <see cref="RootView"/> provides the default implementation.
/// </summary>
public interface IFocus
{
    /// <summary>Moves focus to the next focusable element (same order as Tab).</summary>
    void Next();

    /// <summary>Moves focus to the previous focusable element (same order as Shift+Tab).</summary>
    void Previous();

    /// <summary>
    /// Gets or sets the view that currently holds keyboard focus.
    /// Setting to <c>null</c> clears focus. Setting fires <see cref="View.OnBlur"/> /
    /// <see cref="View.OnFocus"/> on the affected views.
    /// </summary>
    View? FocusedView { get; set; }
}
