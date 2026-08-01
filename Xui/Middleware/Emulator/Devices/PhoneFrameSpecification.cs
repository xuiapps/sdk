using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.Devices;

/// <summary>Logical chrome widths surrounding an emulated phone display.</summary>
internal readonly struct PhoneFrameSpecification(
    NFloat buttonReserve,
    NFloat outerGrayOutlineWidth,
    NFloat screenBlackOutlineWidth)
{
    /// <summary>Space reserved outside the frame for hardware buttons.</summary>
    public NFloat ButtonReserve { get; } = buttonReserve;

    /// <summary>Width of the outer aluminum-gray outline.</summary>
    public NFloat OuterGrayOutlineWidth { get; } = outerGrayOutlineWidth;

    /// <summary>Width of the screen's black outline.</summary>
    public NFloat ScreenBlackOutlineWidth { get; } = screenBlackOutlineWidth;

    /// <summary>Total chrome inset on each side of the logical display.</summary>
    public NFloat TotalInset => ButtonReserve + OuterGrayOutlineWidth + ScreenBlackOutlineWidth;

    /// <summary>Initial iPhone chrome widths in logical pixels.</summary>
    public static PhoneFrameSpecification IPhone { get; } = new(
        buttonReserve: 4,
        outerGrayOutlineWidth: 4,
        screenBlackOutlineWidth: 8);
}
