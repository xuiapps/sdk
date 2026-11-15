using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xui.Apps.TestApp;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class DeviceInfoSnapshotTest
{
    private static TestSinglePageApp<Application, MainWindow> CreateApp(
        TestRuntimeVariant variant,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string testName = "") =>
        IntegrationRuntimeVariants.CreateApp<Application, MainWindow>(
            (600, 400), variant, callerPath, testName, fontSource: Xui.Core.Fonts.Inter.Source);

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator, DevicePlatform.iOS, 393, 852, 59, 34, 44)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator, DevicePlatform.Android, 448, 1008, 40, 34, 28)]
    public void DeviceInfo_Shows_Emulated_Geometry_And_Preferences(
        TestRuntimeVariant variant, DevicePlatform platform,
        int width, int height, int top, int bottom, int cornerRadius)
    {
        using var app = CreateApp(variant);
        app.SvgFontResolver = new EmbeddedFontResolver();
        app.Render();
        var link = Assert.IsAssignableFrom<Button>(app.Window.RootView.FindViewById("DeviceInfo"));
        link.Clicked!();
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(400));
        app.Render();

        var diagram = Assert.IsAssignableFrom<View>(app.Window.RootView.FindViewById("DeviceInfo.SafeArea"));
        var device = Assert.IsAssignableFrom<IDeviceInfo>(diagram.GetService(typeof(IDeviceInfo)));
        Assert.Equal(platform, device.Platform);
        Assert.Equal(DeviceFormFactor.Mobile, device.FormFactor);
        Assert.Equal(PointerModel.Touch, device.PointerModel);
        Assert.Equal(ColorScheme.Light, device.ColorScheme);
        Assert.Equal((nfloat)1, device.AccessibilityFontScale);
        Assert.False(device.PrefersReducedMotion);
        Assert.False(device.PrefersHighContrast);
        Assert.Equal(new Rect(0, 0, width, height), app.Window.DisplayArea);
        Assert.Equal(new Rect(0, top, width, height - top - bottom), app.Window.SafeArea);
        Assert.Equal((nfloat)cornerRadius, app.Window.ScreenCornerRadius);

        app.MarkdownHeading($"Device Info — {platform}", level: 1);
        app.MarkdownParagraph("The real TestApp Device Info page runs inside the phone emulator. " +
            "The emulator supplies the mobile OS, touch input model, preferences, window geometry, and device frame. " +
            "Inter fonts are embedded in each SVG so the layout is readable without locally installed fonts.");
        app.MarkdownList([
            $"Display: {width} × {height} logical points; screen corner radius: {cornerRadius} pt.",
            $"Safe-area insets: top {top} pt, right 0 pt, bottom {bottom} pt, left 0 pt.",
            $"Safe content: {width} × {height - top - bottom} pt, starting at (0, {top}).",
            $"Mocked device: {platform}, Mobile, Touch, Light; font scale 1×; reduce motion and high contrast off.",
        ]);
        app.MarkdownHeading("Window geometry");
        app.MarkdownParagraph("A proportional window outline surrounds the tinted safe-content region. " +
            "Underlined top, right, bottom, and left measurements connect to the center of each safe-area inset band. " +
            "Zero-width insets place their dots on the screen edge. " +
            "Display dimensions and corner radius appear below the diagram.");
        var geometry = TextElements(app.Snapshot("SafeArea"));
        foreach (var label in new[] { "TOP", "RIGHT", "BOTTOM", "LEFT", $"{top} pt", $"{bottom} pt",
            $"{width} × {height} pt", $"Corners {cornerRadius} pt", "SAFE", "CONTENT" })
            Assert.Contains(geometry, element => element.Value == label);
        Assert.Equal(2, geometry.Count(element => element.Value == "0 pt"));
        Assert.All(geometry.Where(element => element.Value.EndsWith(" pt")),
            element => Assert.Equal("Inter", (string?)element.Attribute("font-family")));

        app.MarkdownHeading("Device and accessibility");
        app.MarkdownParagraph("Scroll down to reveal device values, a separate ACCESSIBILITY section for font scale, " +
            "reduce motion, and high contrast, and the emulator runtime. " +
            "Labels stay on the left, values align on the right, and the final row remains inside the safe area.");
        var scroll = Assert.IsType<ScrollView>(app.Window.RootView.FindViewById("DeviceInfo.Scroll"));
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Center.X, scroll.Frame.Bottom - 60));
        var end = start - new Vector(0, 360);
        app.PointerDown(start);
        app.AnimationFrame(TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(600));
        app.PointerMove(end);
        // Hold before release to avoid a fling; then let any edge bounce settle.
        app.AnimationFrame(TimeSpan.FromMilliseconds(600), TimeSpan.FromMilliseconds(1000));
        app.PointerUp(end);
        for (var i = 0; i < 120; i++)
            app.AnimationFrame(TimeSpan.FromSeconds(1 + i / 60d), TimeSpan.FromSeconds(1 + (i + 1) / 60d));

        var details = TextElements(app.Snapshot("DevicePreferences"));
        foreach (var label in new[] { "Operating system", platform.ToString(), "Form factor", "Mobile",
            "Pointer model", "Touch", "Color scheme", "Light", "ACCESSIBILITY", "Font scale", "1×",
            "Reduce motion", "High contrast", "EmulatorWindow" })
            Assert.Contains(details, element => element.Value == label);
        Assert.Equal(2, details.Count(element => element.Value == "Off"));
    }

    private static XElement[] TextElements(string svg) =>
        XDocument.Parse(svg).Descendants(XName.Get("text", "http://www.w3.org/2000/svg")).ToArray();

    private sealed class EmbeddedFontResolver : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) =>
            new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
