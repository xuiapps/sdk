using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Examples;

/// <summary>Displays the device, accessibility, and window geometry information available to Xui views.</summary>
public sealed class DeviceInfoExample : Example
{
    public DeviceInfoExample()
    {
        Title = "Device Info";
        Content = new DeviceInfoView();
    }

    private sealed class DeviceInfoView : ViewCollection
    {
        private readonly VerticalStack rows = new() { Margin = 20 };
        private IDeviceInfo? device;

        public DeviceInfoView() => Add(rows);

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

        protected override Size MeasureCore(Size available, IMeasureContext context) => rows.Measure(available, context);

        protected override void ArrangeCore(Rect rect, IMeasureContext context) => rows.Arrange(rect, context);

        protected override void RenderCore(IContext context) => this.RenderChild(context, rows);

        private void Refresh()
        {
            rows.Clear();
            AddHeading("Device Info");

            if (device is null)
            {
                AddRow("Device information", "Unavailable");
                return;
            }

            AddRow("Operating system", device.Platform.ToString());
            AddRow("Form factor", device.FormFactor.ToString());
            AddRow("Pointer model", device.PointerModel.ToString());
            AddRow("Color scheme", device.ColorScheme.ToString());
            AddRow("Accessibility font scale", device.AccessibilityFontScale.ToString("0.##"));
            AddRow("Reduce motion", device.PrefersReducedMotion ? "Enabled" : "Disabled");
            AddRow("High contrast", device.PrefersHighContrast ? "Enabled" : "Disabled");

            AddHeading("Window Geometry");
            if (this.TryFindParent<RootView>(out var root))
            {
                var window = root.Window;
                AddRow("Runtime", window.Actual?.GetType().Name ?? "Unavailable");
                AddRow("Display area", Format(window.DisplayArea));
                AddRow("Safe area", Format(window.SafeArea));
                AddRow("Screen corner radius", window.ScreenCornerRadius.ToString("0.##"));
            }
            else
            {
                AddRow("Window geometry", "Unavailable");
            }

            Invalidate();
        }

        private void AddHeading(string text) => rows.Add(new Label
        {
            Text = text,
            FontFamily = "Inter",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Margin = (0, 0, 12, 8),
        });

        private void AddRow(string label, string value) => rows.Add(new Label
        {
            Text = $"{label}: {value}",
            FontFamily = "Inter",
            FontSize = 14,
            Margin = (0, 0, 8, 4),
        });

        private static string Format(Rect rect) =>
            $"x {rect.X:0.##}, y {rect.Y:0.##}, {rect.Width:0.##} × {rect.Height:0.##}";
    }
}
