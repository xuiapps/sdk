using Xui.DevKit.UI.Widgets;
using Xui.DevKit.UI.Design;
using Xui.Core.DI;
using Xui.Apps.TestApp;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class OverlayTest
{
    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void Overlay_OpenAndDismiss(TestRuntimeVariant runtimeVariant)
    {
        using var app = IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((800, 560), runtimeVariant,
            SourcePath(), nameof(Overlay_OpenAndDismiss));
        app.Run(() =>
        {
            // Layers is intentionally absent from the mobile menu. Exercise the same
            // window overlay contract in an identified fixture on both runtimes.
            var trigger = new Button { Id = "Overlay.Open", Text = "Open Overlay",
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Middle };
            var device = Assert.IsAssignableFrom<IDeviceInfo>(app.Window.GetService(typeof(IDeviceInfo)));
            var fixture = new ThemeHost(new XuiDesignSystem(new(), device))
                { Id = "Overlay.Fixture", BackgroundColor = Colors.White, Content = trigger };
            app.Window.RootView.Content = fixture;
            app.Prepare();
            using var overlay = Assert.IsAssignableFrom<IOverlay>(trigger.GetService(typeof(IOverlay)));
            trigger.Clicked = () => overlay.Show(new Border
            {
                Id = "Overlay.Content", BackgroundColor = Colors.LightGray,
                Content = new Label { Text = "Overlay content", FontFamily = "Inter", Margin = 16 },
            }, trigger.Frame, PopupPlacement.Below, new Size(200, 120));
            app.MarkdownHeading("Window overlay", 1);
            app.MarkdownParagraph("Tap the identified trigger to open an overlay, then tap outside its bounds to dismiss it.");
            app.Snapshot("Closed");
            app.Tap("Overlay.Open");
            Assert.True(overlay.IsVisible);
            var content = app.RequireView("Overlay.Content", "verifying the open overlay");
            app.Snapshot("Open");
            var outside = fixture.LocalToGlobal(fixture.Frame.TopLeft + new Vector(12, 12));
            Assert.False(content.Frame.Contains(outside));
            app.MouseDown(outside); app.MouseUp(outside); app.Render();
            Assert.False(overlay.IsVisible);
            Assert.Null(app.Window.RootView.FindViewById("Overlay.Content"));
            app.Snapshot("Dismissed");
        });
    }
    private static string SourcePath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

    private sealed class ThemeHost(IDesignSystem theme) : Border
    {
        public override object? GetService(Type type) => type == typeof(IDesignSystem) ? theme : base.GetService(type);
    }
}
