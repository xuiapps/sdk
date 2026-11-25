using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Middleware.Emulator.Devices;
using Xui.Middleware.Emulator.UI.Phone;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator;

// Compiled into the middleware fixture PE to access its actual internal phone renderer.
// This bridge does not replace PhoneFrameView, EmulatedWindow, or the event router.
public sealed class CompilerPhoneHost
{
    private readonly PhoneChromeRenderer chrome = new();
    public double ScreenX, ScreenY, ScreenScale;
    private double width, height;

    public void Begin(IContext context, double width, double height)
    {
        this.width = width; this.height = height;
        var device = DeviceProfiles.IPhone15Pro;
        var specification = PhoneFrameSpecification.IPhone;
        var geometry = PhoneFrameGeometry.Create(new Rect(24, 24, (NFloat)width - 48, (NFloat)height - 48), device, specification);
        ScreenX = (double)geometry.ScreenFrame.X; ScreenY = (double)geometry.ScreenFrame.Y; ScreenScale = (double)geometry.Scale;
        chrome.RenderFrame(context, device, specification, in geometry);
        context.Save();
        context.BeginPath(); context.RoundRect(geometry.ScreenFrame, device.ScreenCornerRadius * geometry.Scale); context.Clip();
        context.Translate(geometry.ScreenFrame.TopLeft); context.Scale((geometry.Scale, geometry.Scale));
    }

    public void End(IContext context)
    {
        context.Restore();
        var device = DeviceProfiles.IPhone15Pro;
        var geometry = PhoneFrameGeometry.Create(new Rect(24, 24, (NFloat)width - 48, (NFloat)height - 48), device, PhoneFrameSpecification.IPhone);
        chrome.RenderDisplayCutout(context, device, in geometry);
    }
}
