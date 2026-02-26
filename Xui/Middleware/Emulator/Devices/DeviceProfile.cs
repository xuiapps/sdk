using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Middleware.Emulator.Devices;

public readonly struct DeviceProfile
{
    public Brand Brand { get; init; }
    public DeviceType DeviceType { get; init; }
    public string Model { get; init; }
    public Size LogicalResolution { get; init; }
    public NFloat ScaleFactor { get; init; }
    public Frame SafeAreaInsetsPortrait { get; init; }
    public Frame SafeAreaInsetsLandscape { get; init; }
    public NotchType NotchType { get; init; }
    public Rect NotchFrame { get; init; }
    public nfloat ScreenCornerRadius { get; init; }
}
