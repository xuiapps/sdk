using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.EmulatorLegacy.Actual;

public partial class EmulatorWindow
{
    public sealed class MenuHandle
    {
        public static readonly MenuHandle Instance = new();

        private MenuHandle() { }

        public void Render(IContext ctx, Point centerBottom) => Render(ctx, centerBottom, 110f, 4f, 1.5f);

        public void Render(IContext ctx, Point centerBottom, NFloat width, NFloat height, NFloat radius)
        {
            ctx.BeginPath();
            ctx.RoundRect(
                new Rect(
                    x: centerBottom.X - width / 2f,
                    y: centerBottom.Y - height,
                    width: width,
                    height: height),
                radius
            );
            ctx.SetFill(0x111111FF);
            ctx.Fill();
        }
    }

    public sealed class PinholeCutout
    {
        public static readonly PinholeCutout Instance = new();

        private PinholeCutout() { }

        public void Render(IContext ctx, Point position) => Render(ctx, position, 90f, 26f, 13f);

        public void Render(IContext ctx, Point position, NFloat width, NFloat height, NFloat radius)
        {
            ctx.BeginPath();
            ctx.RoundRect(new Rect(position.X, position.Y, width, height), radius);
            ctx.SetFill(0x111111FF);
            ctx.Fill();
        }
    }

    public sealed class ClockIcon
    {
        public static readonly ClockIcon Instance = new();

        private ClockIcon() { }

        public void Render(IContext ctx, Point position, DateTime now)
        {
            ctx.Save();
            ctx.Translate(position);
            ctx.SetFill(Colors.Black);
            ctx.TextAlign = TextAlign.Center;
            ctx.TextBaseline = TextBaseline.Top;
            ctx.SetFont(new Font()
            {
                FontFamily = "Verdana",
                FontSize = 12,
                FontWeight = 600,
                FontStyle = FontStyle.Normal,
                LineHeight = 16
            });
            ctx.FillText(now.ToString("H:mm"), (0, 0));
            ctx.Restore();
        }
    }

    public sealed class SignalStrengthIcon
    {
        public static readonly SignalStrengthIcon Instance = new();

        private SignalStrengthIcon() { }

        public void Render(IContext ctx, Point position) =>
            Render(ctx, position, 0.67f);

        public void Render(IContext ctx, Point position, NFloat strength)
        {
            ctx.Save();
            ctx.Translate(position);

            var active = NFloat.Max(0f, NFloat.Min(strength, 1f));
            var mediumOpacity = active >= 0.4f ? 1f : 0.33f;
            var lowOpacity = active >= 0.75f ? 1f : 0.33f;

            ctx.BeginPath();
            ctx.SetStroke(Colors.Black);
            ctx.Arc((0, 0), 2.5f, NFloat.Pi * 1.75f, NFloat.Pi * 1.25f, Winding.CounterClockWise);
            ctx.LineWidth = 4f;
            ctx.Stroke();

            ctx.BeginPath();
            ctx.SetStroke(Colors.Black);
            ctx.Arc((0, 0), 7.5f, NFloat.Pi * 1.75f, NFloat.Pi * 1.25f, Winding.CounterClockWise);
            ctx.LineWidth = 2f;
            ctx.Stroke();

            ctx.BeginPath();
            ctx.SetStroke(new Color(0, 0, 0, mediumOpacity));
            ctx.Arc((0, 0), 11.5f, NFloat.Pi * 1.75f, NFloat.Pi * 1.25f, Winding.CounterClockWise);
            ctx.LineWidth = 2f;
            ctx.Stroke();

            if (active >= 0.75f)
            {
                ctx.BeginPath();
                ctx.SetStroke(new Color(0, 0, 0, lowOpacity));
                ctx.Arc((0, 0), 15.5f, NFloat.Pi * 1.75f, NFloat.Pi * 1.25f, Winding.CounterClockWise);
                ctx.LineWidth = 2f;
                ctx.Stroke();
            }

            ctx.Restore();
        }
    }

    public sealed class BatteryIcon
    {
        public static readonly BatteryIcon Instance = new();

        private BatteryIcon() { }

        public void Render(IContext ctx, Point position) =>
            Render(ctx, position, 0.65f);

        public void Render(IContext ctx, Point position, NFloat level)
        {
            ctx.Save();
            ctx.Translate(position);

            ctx.BeginPath();
            ctx.RoundRect(new Rect(0, 0, 20, 10), 3f);
            ctx.LineWidth = 1f;
            ctx.SetStroke(Colors.Black);
            ctx.Stroke();

            ctx.BeginPath();
            var clampedLevel = NFloat.Max(0f, NFloat.Min(level, 1f));
            var fillWidth = 12f * clampedLevel;
            ctx.RoundRect(new Rect(1.5f, 1.5f, fillWidth, 7), 2f);
            ctx.SetFill(Colors.Black);
            ctx.Fill();

            ctx.Restore();
        }
    }

    public sealed class FiveGIcon
    {
        public static readonly FiveGIcon Instance = new();

        private FiveGIcon() { }

        public void Render(IContext ctx, Point position, string label = "5G")
        {
            ctx.Save();
            ctx.Translate(position);
            ctx.SetFill(Colors.Black);
            ctx.TextAlign = TextAlign.Center;
            ctx.SetFont(new Font
            {
                FontFamily = "Verdana",
                FontSize = 12,
                FontWeight = 600,
                FontStyle = FontStyle.Normal,
                LineHeight = 16
            });
            ctx.FillText(label, (0, 0));
            ctx.Restore();
        }
    }
}
