using System.Runtime.InteropServices;
using Xui.Core.Math2D;
using Xui.Middleware.EmulatorLegacy.Devices;

namespace Xui.Middleware.EmulatorLegacy.Actual;

internal readonly struct EmulatorGeometry
{
    public readonly NFloat GrayOutlineWidth;
    public readonly NFloat BlackOutlineWidth;
    public readonly NFloat ButtonOutlineWidth;
    public readonly NFloat ScreenCornerRadius;
    public readonly NFloat Scale;
    public readonly Rect EmulatorRect;

    private EmulatorGeometry(
        NFloat grayOutlineWidth,
        NFloat blackOutlineWidth,
        NFloat buttonOutlineWidth,
        NFloat screenCornerRadius,
        NFloat scale,
        Rect emulatorRect)
    {
        GrayOutlineWidth = grayOutlineWidth;
        BlackOutlineWidth = blackOutlineWidth;
        ButtonOutlineWidth = buttonOutlineWidth;
        ScreenCornerRadius = screenCornerRadius;
        Scale = scale;
        EmulatorRect = emulatorRect;
    }

    public static EmulatorGeometry Create(Rect phoneFrame, DeviceProfile device)
    {
        var scale = NFloat.Min(
            phoneFrame.Width / device.LogicalResolution.Width,
            phoneFrame.Height / device.LogicalResolution.Height);
        var grayOutlineWidth = ClampOutline(12f * scale, 6f);
        var blackOutlineWidth = ClampOutline(18f * scale, 9f);
        var buttonOutlineWidth = ClampOutline(6f * scale, 6f);
        NFloat corner = device.ScreenCornerRadius * scale;

        var emulatorWidth = device.LogicalResolution.Width * scale;
        var emulatorHeight = device.LogicalResolution.Height * scale;
        var emulatorX = phoneFrame.X + (phoneFrame.Width - emulatorWidth) * 0.5f;
        var emulatorY = phoneFrame.Y + (phoneFrame.Height - emulatorHeight) * 0.5f;

        var emulatorRect = new Rect(emulatorX, emulatorY, emulatorWidth, emulatorHeight);

        return new EmulatorGeometry(
            grayOutlineWidth,
            blackOutlineWidth,
            buttonOutlineWidth,
            corner,
            scale,
            emulatorRect);
    }

    public static EmulatorGeometry CreateSnapshot(DeviceProfile device)
    {
        NFloat grayOutlineWidth = 6f;
        // Snapshot rendering is unscaled: retain the full nominal 3px screen-glass line.
        NFloat blackOutlineWidth = 9f;
        NFloat buttonOutlineWidth = 6f;
        NFloat corner = device.ScreenCornerRadius;
        NFloat margin = 24f;

        var emulatorRect = new Rect(
            margin,
            margin,
            device.LogicalResolution.Width,
            device.LogicalResolution.Height);
        return new EmulatorGeometry(
            grayOutlineWidth,
            blackOutlineWidth,
            buttonOutlineWidth,
            corner,
            1,
            emulatorRect);
    }

    public Point MapEmulatorToHost(Point emulatorPoint) =>
        EmulatorRect.TopLeft + new Vector(emulatorPoint.X * Scale, emulatorPoint.Y * Scale);

    public Point MapHostToEmulator(Point hostPoint) =>
        new Point(
            (hostPoint.X - EmulatorRect.X) / Scale,
            (hostPoint.Y - EmulatorRect.Y) / Scale);

    public bool TryMapHostToEmulator(Point hostPoint, out Point emulatorPoint)
    {
        emulatorPoint = MapHostToEmulator(hostPoint);
        return EmulatorRect.Contains(hostPoint);
    }

    private static NFloat ClampOutline(NFloat width, NFloat maximum) => NFloat.Clamp(width, 1f, maximum);
}
