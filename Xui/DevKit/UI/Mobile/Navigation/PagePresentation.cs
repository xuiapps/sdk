using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Mobile.Navigation;

/// <summary>Presentation preferences supplied by a page to its navigation host.</summary>
public readonly record struct PagePresentation
{
    /// <summary>Creates the default opaque, device-clipped page presentation with its transition shadow enabled.</summary>
    public PagePresentation()
    {
        Background = null;
        Clip = PageClipMode.Device;
        UsesTransitionShadow = true;
    }

    /// <summary>Creates a page presentation with explicit surface, clipping, and shadow preferences.</summary>
    public PagePresentation(Color? background, PageClipMode clip, bool usesTransitionShadow)
    {
        Background = background;
        Clip = clip;
        UsesTransitionShadow = usesTransitionShadow;
    }

    /// <summary>Gets the page background, or <see langword="null"/> to use the navigation default.</summary>
    public Color? Background { get; init; }

    /// <summary>Gets the clipping applied to the page.</summary>
    public PageClipMode Clip { get; init; }

    /// <summary>Gets whether the transition shadow is drawn for this page.</summary>
    public bool UsesTransitionShadow { get; init; }
}
