using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Middleware.Emulator.Devices;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.UI.Phone;

/// <summary>Draws the static hardware surrounding an emulated phone display.</summary>
internal sealed class PhoneChromeRenderer
{
    private static readonly Color AluminumColor = new(0xB8B8BEFF);
    private static readonly Color ScreenBezelColor = new(0x111111FF);

    public void RenderFrame(IContext context, DeviceProfile device, PhoneFrameSpecification specification, in PhoneFrameGeometry geometry)
    {
        RenderSideButtons(context, specification, in geometry);

        context.BeginPath();
        context.RoundRect(
            geometry.AluminumFrame,
            (device.ScreenCornerRadius + specification.ScreenBlackOutlineWidth + specification.OuterGrayOutlineWidth) * geometry.Scale);
        context.SetFill(AluminumColor);
        context.Fill();

        context.BeginPath();
        context.RoundRect(
            geometry.BlackScreenFrame,
            (device.ScreenCornerRadius + specification.ScreenBlackOutlineWidth) * geometry.Scale);
        context.SetFill(ScreenBezelColor);
        context.Fill();

        context.BeginPath();
        context.RoundRect(geometry.ScreenFrame, device.ScreenCornerRadius * geometry.Scale);
        context.SetFill(Colors.White);
        context.Fill();
    }

    public void RenderDisplayCutout(IContext context, DeviceProfile device, in PhoneFrameGeometry geometry)
    {
        if (device.NotchType == NotchType.None)
            return;

        var cutout = new Rect(
            geometry.ScreenFrame.X + device.NotchFrame.X * geometry.Scale,
            geometry.ScreenFrame.Y + device.NotchFrame.Y * geometry.Scale,
            device.NotchFrame.Width * geometry.Scale,
            device.NotchFrame.Height * geometry.Scale);

        context.BeginPath();
        switch (device.NotchType)
        {
            case NotchType.PinHole:
            {
                var radius = NFloat.Min(cutout.Width, cutout.Height) * .5f;
                context.Ellipse(cutout.Center, radius, radius, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
                break;
            }
            default:
                context.RoundRect(cutout, cutout.Height * .5f);
                break;
        }

        context.SetFill(ScreenBezelColor);
        context.Fill();
    }

    private static void RenderSideButtons(IContext context, PhoneFrameSpecification specification, in PhoneFrameGeometry geometry)
    {
        var buttonWidth = specification.ButtonReserve * geometry.Scale;
        var buttonRadius = NFloat.Max(1, buttonWidth * .5f);
        var left = geometry.OuterFrame.Left;
        var right = geometry.OuterFrame.Right - buttonWidth;

        RenderButton(context, new Rect(left, geometry.OuterFrame.Top + geometry.OuterFrame.Height * .20f, buttonWidth, geometry.OuterFrame.Height * .09f), buttonRadius);
        RenderButton(context, new Rect(left, geometry.OuterFrame.Top + geometry.OuterFrame.Height * .33f, buttonWidth, geometry.OuterFrame.Height * .13f), buttonRadius);
        RenderButton(context, new Rect(right, geometry.OuterFrame.Top + geometry.OuterFrame.Height * .31f, buttonWidth, geometry.OuterFrame.Height * .21f), buttonRadius);
    }

    private static void RenderButton(IContext context, Rect rect, NFloat radius)
    {
        context.BeginPath();
        context.RoundRect(rect, radius);
        context.SetFill(AluminumColor);
        context.Fill();
    }
}
