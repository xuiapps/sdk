namespace Xui.DevKit.UI.Design;

/// <summary>
/// Controls the relationship between Application and Surface background colors.
/// </summary>
public enum NeutralStyle
{
    /// <summary>
    /// Both Application and Surface are desaturated neutral gray. No hue tint.
    /// </summary>
    Monochrome,

    /// <summary>
    /// Application gets a subtle Secondary hue tint, Surface stays neutral.
    /// Suitable for apps with many panels/surfaces where the app background groups them.
    /// </summary>
    SecondaryApp,

    /// <summary>
    /// Surface gets a subtle Secondary hue tint, Application stays neutral.
    /// Suitable when few panels appear and most widgets sit directly on the app background.
    /// </summary>
    SecondarySurface,

    /// <summary>
    /// Application gets a subtle Tertiary hue tint, Surface stays neutral.
    /// </summary>
    TertiaryApp,

    /// <summary>
    /// Surface gets a subtle Tertiary hue tint, Application stays neutral.
    /// </summary>
    TertiarySurface,
}
