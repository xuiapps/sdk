using Xui.Core.Math2D;
using Xui.Middleware.Emulator.Devices;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.UI.Phone;

/// <summary>Calculated host-space geometry for a uniformly scaled phone frame.</summary>
internal readonly struct PhoneFrameGeometry
{
    private PhoneFrameGeometry(NFloat scale, Rect outerFrame, Rect aluminumFrame, Rect blackScreenFrame, Rect screenFrame)
    {
        Scale = scale;
        OuterFrame = outerFrame;
        AluminumFrame = aluminumFrame;
        BlackScreenFrame = blackScreenFrame;
        ScreenFrame = screenFrame;
    }

    public NFloat Scale { get; }
    public Rect OuterFrame { get; }
    public Rect AluminumFrame { get; }
    public Rect BlackScreenFrame { get; }
    public Rect ScreenFrame { get; }

    public static PhoneFrameGeometry Create(Rect availableFrame, DeviceProfile device, PhoneFrameSpecification specification)
    {
        var inset = specification.TotalInset;
        var logicalFrameSize = new Size(
            device.LogicalResolution.Width + inset * 2,
            device.LogicalResolution.Height + inset * 2);
        var scale = NFloat.Min(
            availableFrame.Width / logicalFrameSize.Width,
            availableFrame.Height / logicalFrameSize.Height);
        var outerFrame = new Rect(
            availableFrame.X + (availableFrame.Width - logicalFrameSize.Width * scale) * .5f,
            availableFrame.Y + (availableFrame.Height - logicalFrameSize.Height * scale) * .5f,
            logicalFrameSize.Width * scale,
            logicalFrameSize.Height * scale);
        var aluminumFrame = outerFrame.Expand(-specification.ButtonReserve * scale);
        var blackScreenFrame = aluminumFrame.Expand(-specification.OuterGrayOutlineWidth * scale);
        var screenFrame = blackScreenFrame.Expand(-specification.ScreenBlackOutlineWidth * scale);

        return new PhoneFrameGeometry(scale, outerFrame, aluminumFrame, blackScreenFrame, screenFrame);
    }

    public Point MapHostToDevice(Point point) => new(
        (point.X - ScreenFrame.X) / Scale,
        (point.Y - ScreenFrame.Y) / Scale);
}
