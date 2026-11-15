using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.Devices;

/// <summary>Logical display and hardware-cutout data for an emulated device.</summary>
internal readonly struct DeviceProfile
{
    public string Model { get; init; }
    public Size LogicalResolution { get; init; }
    public Frame SafeAreaInsetsPortrait { get; init; }
    public Frame SafeAreaInsetsLandscape { get; init; }
    public NotchType NotchType { get; init; }
    public Rect NotchFrame { get; init; }
    public NFloat ScreenCornerRadius { get; init; }
}
