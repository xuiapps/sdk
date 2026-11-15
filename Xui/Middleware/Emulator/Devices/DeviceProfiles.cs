using Xui.Core.Math2D;

namespace Xui.Middleware.Emulator.Devices;

/// <summary>The initial device profiles supported by the emulator.</summary>
internal static class DeviceProfiles
{
    public static DeviceProfile IPhone15Pro { get; } = new()
    {
        Model = "iPhone 15 Pro",
        LogicalResolution = new Size(393, 852),
        SafeAreaInsetsPortrait = new Frame(59, 0, 34, 0),
        SafeAreaInsetsLandscape = new Frame(0, 59, 21, 59),
        NotchType = NotchType.DynamicIsland,
        NotchFrame = new Rect(133.5f, 11, 126, 36),
        ScreenCornerRadius = 44,
    };
}
