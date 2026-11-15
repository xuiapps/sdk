using System.Runtime.InteropServices;
using Xui.Core.Abstract;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Middleware.EmulatorLegacy.Devices;

namespace Xui.Middleware.EmulatorLegacy.Actual;

internal sealed class EmulatorChromeRenderer
{
    private static readonly Color AluminumColor = new(0xB8B8BEFF);

    public void Render(
        IContext ctx,
        in EmulatorGeometry geometry,
        in Rect hostRect,
        DeviceProfile device,
        in EmulatorStatusBarStyle statusBarStyle,
        IClock clock)
    {
        var screenRect = geometry.EmulatorRect;
        var blackOutline = geometry.BlackOutlineWidth;
        var grayOutline = geometry.GrayOutlineWidth;
        var buttonOutline = geometry.ButtonOutlineWidth;
        var blackFrame = screenRect.Expand(blackOutline * 0.5f);
        // The gray line starts exactly where the black glass line ends. Hardware buttons
        // occupy the reserved band outside this aluminum edge.
        var aluminumFrame = screenRect.Expand(blackOutline + grayOutline * 0.5f);

        // Hardware is below both rings so the screen and aluminum lines cleanly cover its joins.
        if (device.Brand == Brand.Apple && device.DeviceType == DeviceType.Phone)
            RenderIPhoneHardware(ctx, in geometry, in aluminumFrame);

        // The inner black line belongs to the glass; the outer gray line is the aluminum edge.
        ctx.BeginPath();
        ctx.RoundRect(
            rect: blackFrame,
            radius: geometry.ScreenCornerRadius + blackOutline * 0.5f);
        ctx.SetStroke(new Color(0x111111FF));
        ctx.LineWidth = blackOutline;
        ctx.Stroke();

        ctx.BeginPath();
        ctx.RoundRect(
            rect: aluminumFrame,
            radius: geometry.ScreenCornerRadius + blackOutline + buttonOutline + grayOutline * 0.5f);
        ctx.SetStroke(AluminumColor);
        ctx.LineWidth = grayOutline;
        ctx.Stroke();

        NFloat phoneToTabletT = Easing.Normalize(hostRect.Width, 500, 575);
        var iconTop = geometry.EmulatorRect.Y + 12f;
        var cutoutRect = new Rect(
            geometry.EmulatorRect.X + device.NotchFrame.X * geometry.Scale,
            geometry.EmulatorRect.Y + device.NotchFrame.Y * geometry.Scale,
            device.NotchFrame.Width * geometry.Scale,
            device.NotchFrame.Height * geometry.Scale);
        RenderCameraCutout(ctx, device, cutoutRect);

        NFloat clockX = NFloat.Lerp(
            (geometry.EmulatorRect.Center.X - 22f) / 2,
            (300 / 2f - 22f) / 2,
            Easing.EaseInOutSine(phoneToTabletT));
        EmulatorWindow.ClockIcon.Instance.Render(ctx, (clockX, iconTop + 6f), clock.Now);

        NFloat instrumentsX = NFloat.Lerp(
            geometry.EmulatorRect.Center.X + 45f + (geometry.EmulatorRect.Center.X - 45f - 22f) / 2f,
            hostRect.Width - 80f,
            Easing.EaseInOutSine(phoneToTabletT));

        EmulatorWindow.SignalStrengthIcon.Instance.Render(
            ctx,
            (instrumentsX - 12f - 8f, iconTop + 19.5f),
            statusBarStyle.SignalStrength ?? 0.67f);
        EmulatorWindow.BatteryIcon.Instance.Render(
            ctx,
            (instrumentsX - 8f, iconTop + 8.5f),
            statusBarStyle.BatteryLevel ?? 0.65f);
        EmulatorWindow.FiveGIcon.Instance.Render(
            ctx,
            (instrumentsX + 36f - 8f, iconTop + 6f),
            statusBarStyle.NetworkText ?? "5G");

        // Menu Handle
        EmulatorWindow.MenuHandle.Instance.Render(ctx, (
            geometry.EmulatorRect.Center.X,
            geometry.EmulatorRect.Bottom - 3f
        ));
    }

    private static void RenderIPhoneHardware(IContext context, in EmulatorGeometry geometry, in Rect aluminumFrame)
    {
        var grayOutline = geometry.GrayOutlineWidth;
        var buttonOutline = geometry.ButtonOutlineWidth;
        var sideX = aluminumFrame.Left - buttonOutline;
        var rightX = aluminumFrame.Right;
        var top = aluminumFrame.Top;

        // Volume up/down and the side button sit in the reserved button-outline band.
        RenderSideButton(context, new Rect(sideX, top + aluminumFrame.Height * .20f, buttonOutline * 1.75f, aluminumFrame.Height * .09f), grayOutline);
        RenderSideButton(context, new Rect(sideX, top + aluminumFrame.Height * .33f, buttonOutline * 1.75f, aluminumFrame.Height * .13f), grayOutline);
        RenderSideButton(context, new Rect(rightX - buttonOutline * .75f, top + aluminumFrame.Height * .31f, buttonOutline * 1.75f, aluminumFrame.Height * .21f), grayOutline);

        // Bright seam ticks prevent the aluminum edge from reading as one continuous dark ring.
        context.SetStroke(new Color(0xF8F8FAFF));
        context.LineWidth = NFloat.Max(1f, grayOutline * .65f);
        RenderSeam(context, aluminumFrame.Left, top + aluminumFrame.Height * .18f, buttonOutline * 1.5f);
        RenderSeam(context, aluminumFrame.Left, top + aluminumFrame.Height * .31f, buttonOutline * 1.5f);
        RenderSeam(context, aluminumFrame.Left, top + aluminumFrame.Height * .48f, buttonOutline * 1.5f);
        RenderSeam(context, aluminumFrame.Right, top + aluminumFrame.Height * .29f, buttonOutline * 1.5f);
        RenderSeam(context, aluminumFrame.Right, top + aluminumFrame.Height * .54f, buttonOutline * 1.5f);
    }

    private static void RenderSideButton(IContext context, Rect rect, NFloat outlineWidth)
    {
        context.BeginPath();
        context.RoundRect(rect, outlineWidth);
        context.SetFill(AluminumColor);
        context.Fill();
        context.SetStroke(new Color(0x77777DFF));
        context.LineWidth = outlineWidth;
        context.Stroke();
    }

    private static void RenderSeam(IContext context, NFloat x, NFloat y, NFloat length)
    {
        context.BeginPath();
        context.MoveTo((x, y));
        context.LineTo((x, y + length));
        context.Stroke();
    }

    private static void RenderCameraCutout(IContext ctx, DeviceProfile device, Rect cutoutRect)
    {
        if (device.NotchType == NotchType.None || cutoutRect.Width <= 0 || cutoutRect.Height <= 0)
            return;

        ctx.BeginPath();
        switch (device.NotchType)
        {
            case NotchType.PinHole:
            {
                var radius = NFloat.Min(cutoutRect.Width, cutoutRect.Height) * 0.5f;
                ctx.Ellipse(cutoutRect.Center, radius, radius, 0, 0, NFloat.Pi * 2f, Winding.ClockWise);
                break;
            }
            case NotchType.DynamicIsland:
            {
                var radius = cutoutRect.Height * 0.5f;
                ctx.RoundRect(cutoutRect, radius);
                break;
            }
            case NotchType.Waterdrop:
                RenderWaterdropCutout(ctx, cutoutRect);
                break;
            case NotchType.Notch:
                RenderNotchCutout(ctx, cutoutRect);
                break;
            default:
            {
                var radius = NFloat.Min(cutoutRect.Height * 0.5f, 10f);
                ctx.RoundRect(cutoutRect, radius);
                break;
            }
        }

        ctx.SetFill(0x111111FF);
        ctx.Fill();
    }

    private static void RenderNotchCutout(IContext ctx, Rect cutoutRect)
    {
        var bottomRadius = NFloat.Min(cutoutRect.Height * 0.5f, cutoutRect.Width * 0.18f);
        ctx.RoundRect(cutoutRect, new CornerRadius(0, 0, bottomRadius, bottomRadius));
    }

    private static void RenderWaterdropCutout(IContext ctx, Rect cutoutRect)
    {
        var shoulderY = cutoutRect.Top + cutoutRect.Height * 0.22f;
        var centerX = cutoutRect.Center.X;
        var lobeControlX = cutoutRect.Width * 0.28f;

        ctx.MoveTo((cutoutRect.Left, cutoutRect.Top));
        ctx.LineTo((cutoutRect.Right, cutoutRect.Top));
        ctx.LineTo((cutoutRect.Right, shoulderY));
        ctx.CurveTo(
            (cutoutRect.Right, cutoutRect.Bottom),
            (centerX + lobeControlX, cutoutRect.Bottom),
            (centerX, cutoutRect.Bottom));
        ctx.CurveTo(
            (centerX - lobeControlX, cutoutRect.Bottom),
            (cutoutRect.Left, cutoutRect.Bottom),
            (cutoutRect.Left, shoulderY));
        ctx.ClosePath();
    }
}
