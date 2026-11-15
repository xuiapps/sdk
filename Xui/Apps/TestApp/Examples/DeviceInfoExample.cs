using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples;

/// <summary>Displays device capabilities and a live diagram of the window's safe area.</summary>
public sealed class DeviceInfoExample : Example
{
    public DeviceInfoExample()
    {
        Title = "Device Info";
        Content = new ScrollView
        {
            Id = "DeviceInfo.Scroll",
            Direction = ScrollDirection.Vertical,
            Content = new DeviceInfoView(),
        };
    }

    private sealed class DeviceInfoView : VerticalStack
    {
        private IDeviceInfo? device;

        public DeviceInfoView() => Margin = new Frame { Top = 24, Right = 20, Bottom = 28, Left = 20 };

        protected override void OnActivate()
        {
            base.OnActivate();
            device = GetService(typeof(IDeviceInfo)) as IDeviceInfo;
            if (device is not null)
                device.Changed += Refresh;
            Refresh();
        }

        protected override void OnDeactivate()
        {
            if (device is not null)
                device.Changed -= Refresh;
            device = null;
            base.OnDeactivate();
        }

        private void Refresh()
        {
            Clear();
            Add(Heading("MADE TO FIT"));
            Add(Text("Room for your content.", 24, FontWeight.SemiBold));
            var introduction = Text("Live window geometry, in logical points.", 12, color: Muted);
            introduction.Margin = new Frame { Top = 8, Bottom = 20 };
            Add(introduction);
            Add(new SafeAreaDiagram { Id = "DeviceInfo.SafeArea", Margin = new Frame { Bottom = 28 } });
            Add(Heading("DEVICE"));

            if (device is null)
                Add(new DetailRow("Device information", "Unavailable"));
            else
            {
                Add(new DetailRow("Operating system", device.Platform.ToString()));
                Add(new DetailRow("Form factor", device.FormFactor.ToString()));
                Add(new DetailRow("Pointer model", device.PointerModel.ToString()));
                Add(new DetailRow("Color scheme", device.ColorScheme.ToString()));
                var accessibility = Heading("ACCESSIBILITY");
                accessibility.Margin = new Frame { Top = 24, Bottom = 18 };
                Add(accessibility);
                Add(new DetailRow("Font scale", $"{device.AccessibilityFontScale:0.##}×"));
                Add(new DetailRow("Reduce motion", device.PrefersReducedMotion ? "On" : "Off"));
                Add(new DetailRow("High contrast", device.PrefersHighContrast ? "On" : "Off"));
            }

            if (this.TryFindParent<RootView>(out var root))
            {
                var runtime = Text(root.Window.Actual?.GetType().Name ?? "Unavailable", 11, color: Muted);
                runtime.Margin = new Frame { Top = 20 };
                Add(runtime);
            }
            Invalidate();
        }
    }

    private sealed class DetailRow : ViewCollection
    {
        private readonly Label name;
        private readonly Label value;

        public DetailRow(string name, string value)
        {
            Add(this.name = Text(name, 13, color: Muted));
            Add(this.value = Text(value, 13, FontWeight.Medium));
            this.value.HorizontalAlignment = HorizontalAlignment.Right;
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            name.Measure((available.Width / 2, 40), context);
            value.Measure((available.Width / 2, 40), context);
            return (available.Width, 40);
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            name.Arrange((rect.X, rect.Y + 11, rect.Width / 2, 18), context);
            value.Arrange((rect.X + rect.Width / 2, rect.Y + 11, rect.Width / 2, 18), context);
        }

        protected override void RenderCore(IContext context)
        {
            context.SetStroke(new Color(0xE9ECEFFF));
            context.LineWidth = 1;
            context.BeginPath();
            context.MoveTo((Frame.Left, Frame.Bottom));
            context.LineTo((Frame.Right, Frame.Bottom));
            context.Stroke();
            base.RenderCore(context);
        }
    }

    private sealed class SafeAreaDiagram : View
    {
        private static readonly Color Surface = new(0xF6F8FAFF);
        private static readonly Color Edge = new(0xD9DFE7FF);

        protected override Size MeasureCore(Size available, IMeasureContext context) => (available.Width, 432);

        protected override void RenderCore(IContext ctx)
        {
            if (!this.TryFindParent<RootView>(out var root))
                return;

            // Read current geometry at render time: resizing or rotating must not leave stale values.
            var window = root.Window;
            var display = window.DisplayArea;
            var safe = window.SafeArea;
            if (display.Width <= 0 || display.Height <= 0)
                return;

            var top = NFloat.Clamp(safe.Top - display.Top, 0, display.Height);
            var bottom = NFloat.Clamp(display.Bottom - safe.Bottom, 0, display.Height - top);
            var left = NFloat.Clamp(safe.Left - display.Left, 0, display.Width);
            var right = NFloat.Clamp(display.Right - safe.Right, 0, display.Width - left);
            var scale = NFloat.Min(NFloat.Max(40, Frame.Width - 152) / display.Width, 264 / display.Height);
            var phone = new Rect(Frame.Center.X - display.Width * scale / 2,
                Frame.Top + 208 - display.Height * scale / 2, display.Width * scale, display.Height * scale);
            var content = new Rect(phone.Left + left * scale, phone.Top + top * scale,
                phone.Width - (left + right) * scale, phone.Height - (top + bottom) * scale);
            var radius = NFloat.Min(window.ScreenCornerRadius * scale, NFloat.Min(phone.Width, phone.Height) / 2);

            ctx.Save();
            ctx.BeginPath(); ctx.RoundRect(Frame, 20); ctx.SetFill(Surface); ctx.Fill();
            DrawText(ctx, "SAFE AREA / INSETS", (Frame.Left + 16, Frame.Top + 22), 10, Muted);

            ctx.Save();
            ctx.BeginPath(); ctx.RoundRect(phone, radius); ctx.Clip();
            ctx.SetFill(PaleRose); ctx.FillRect(phone);
            ctx.SetFill(PaleTeal); ctx.FillRect(content);
            ctx.SetStroke(Teal); ctx.LineWidth = 1;
            Line(ctx, (phone.Left, content.Top), (phone.Right, content.Top));
            Line(ctx, (phone.Left, content.Bottom), (phone.Right, content.Bottom));
            Line(ctx, (content.Left, phone.Top), (content.Left, phone.Bottom));
            Line(ctx, (content.Right, phone.Top), (content.Right, phone.Bottom));
            ctx.Restore();

            ctx.BeginPath(); ctx.RoundRect(phone, radius); ctx.SetStroke(Ink); ctx.LineWidth = 2; ctx.Stroke();
            DrawText(ctx, "SAFE", (content.Center.X, content.Center.Y - 10), 11, Teal, TextAlign.Center);
            DrawText(ctx, "CONTENT", (content.Center.X, content.Center.Y + 6), 11, Teal, TextAlign.Center);

            // Each leader ends at the center of its inset band; zero insets stay on the edge.
            Callout(ctx, "TOP", top, (Frame.Left + 16, Frame.Top + 48),
                (phone.Center.X, (phone.Top + content.Top) / 2), true);
            Callout(ctx, "LEFT", left, (Frame.Left + 16, phone.Center.Y - 38),
                ((phone.Left + content.Left) / 2, phone.Center.Y), true);
            Callout(ctx, "RIGHT", right, (Frame.Right - 76, phone.Center.Y + 12),
                ((phone.Right + content.Right) / 2, phone.Center.Y), false);
            Callout(ctx, "BOTTOM", bottom, (Frame.Right - 76, Frame.Top + 346),
                (phone.Center.X, (phone.Bottom + content.Bottom) / 2), false);

            ctx.SetStroke(Edge); ctx.LineWidth = 1;
            Line(ctx, (Frame.Left + 16, Frame.Top + 394), (Frame.Right - 16, Frame.Top + 394));
            DrawText(ctx, $"{display.Width:0.#} × {display.Height:0.#} pt", (Frame.Left + 16, Frame.Top + 415), 12, Ink);
            DrawText(ctx, $"Corners {window.ScreenCornerRadius:0.#} pt", (Frame.Right - 16, Frame.Top + 415), 11, Muted, TextAlign.Right);
            ctx.Restore();
        }

        private static void Callout(IContext ctx, string label, NFloat inset, Point origin, Point target, bool onLeft)
        {
            DrawText(ctx, label, origin, 9, Muted);
            DrawText(ctx, $"{inset:0.#} pt", (origin.X, origin.Y + 18), 14, Ink);
            var y = origin.Y + 24;
            var end = onLeft ? origin.X + 60 : origin.X;
            ctx.SetStroke(Teal); ctx.LineWidth = 1;
            Line(ctx, (origin.X, y), (origin.X + 60, y));
            ctx.BeginPath();
            ctx.MoveTo((end, y));
            ctx.LineTo(((end + target.X) / 2, y));
            ctx.LineTo((target.X, target.Y));
            ctx.Stroke();
            ctx.SetFill(Teal);
            ctx.BeginPath(); ctx.Arc(target, 2, 0, NFloat.Pi * 2); ctx.Fill();
        }

        private static void Line(IContext ctx, Point from, Point to)
        {
            ctx.BeginPath(); ctx.MoveTo(from); ctx.LineTo(to); ctx.Stroke();
        }

        private static void DrawText(IContext ctx, string text, Point position, NFloat size, Color color, TextAlign align = TextAlign.Left)
        {
            ctx.SetFont(new Font(size, "Inter", FontWeight.Medium));
            ctx.TextAlign = align;
            ctx.TextBaseline = TextBaseline.Alphabetic;
            ctx.SetFill(color);
            ctx.FillText(text, position);
        }
    }
}
