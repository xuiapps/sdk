using Xui.Core.DI;
using Xui.DevKit.UI.Design;
using Xunit;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Integration.DesignSystem;

public class DesignSystemUpdateTest
{
    [Theory]
    [InlineData(DesignSystemChange.Colors)]
    [InlineData(DesignSystemChange.Shape)]
    [InlineData(DesignSystemChange.Spacing)]
    [InlineData(DesignSystemChange.Typography)]
    [InlineData(DesignSystemChange.Motion)]
    public void OnlyChangedGroupsAreUpdatedAndNotified(DesignSystemChange expected)
    {
        var device = new Device();
        var theme = new XuiDesignSystem(new(), device);
        var before = Groups(theme);
        var notifications = new List<DesignSystemChange>();
        theme.Changed += notifications.Add;
        var options = expected switch
        {
            DesignSystemChange.Colors => new XuiDesignSystemOptions { PrimaryHue = 90 },
            DesignSystemChange.Shape => new XuiDesignSystemOptions { Shape = ShapePreset.Square },
            DesignSystemChange.Spacing => new XuiDesignSystemOptions { Sizing = SizingPreset.Desktop },
            DesignSystemChange.Typography => new XuiDesignSystemOptions { DefaultFontFamily = "Another font" },
            _ => new XuiDesignSystemOptions { Motion = MotionPreset.Long },
        };
        theme.Update(options, device);
        Assert.Equal(expected, Assert.Single(notifications));
        var after = Groups(theme);
        foreach (var (kind, group) in before)
            if (kind == expected && kind != DesignSystemChange.Colors) Assert.NotSame(group, after[kind]);
            else Assert.Same(group, after[kind]);
        Assert.Same(theme.Colors.Primary, theme.Colors.Primary); // Shared role object, not a copied value bundle.
        theme.Update(options, device);
        Assert.Single(notifications);
    }

    [Fact]
    public void HueUpdatesKeepThePaletteAndRolesAndAllocateNothing()
    {
        var device = new Device();
        var theme = new XuiDesignSystem(new(), device);
        var palette = theme.Colors;
        var roles = new[] { palette.Primary, palette.Secondary, palette.Tertiary, palette.Warning,
            palette.Error, palette.Application, palette.Surface, palette.Neutral };
        var original = palette.Primary.Background;
        var changes = 0;
        theme.Changed += _ => changes++;
        for (var i = 0; i < 32; i++) theme.Update(new() { PrimaryHue = i }, device);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 32; i < 96; i++)
        {
            theme.Update(new() { PrimaryHue = i }, device);
            // Lazy interaction colors also live in the retained shared group.
            _ = palette.Primary.HoverBackground;
            _ = palette.Primary.PressedBackground;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
        Assert.Equal(95, changes);
        Assert.Same(palette, theme.Colors);
        var updated = new[] { palette.Primary, palette.Secondary, palette.Tertiary, palette.Warning,
            palette.Error, palette.Application, palette.Surface, palette.Neutral };
        for (var i = 0; i < roles.Length; i++) Assert.Same(roles[i], updated[i]);
        Assert.NotEqual(original, palette.Primary.Background);
        Assert.Equal(palette.Primary.Ramp[NFloat.Clamp(palette.Primary.BackgroundLightness - 0.06f, 0, 1)], palette.Primary.HoverBackground);
    }

    [Fact]
    public void EquivalentOptionsDoNotRebuildOrNotify()
    {
        var device = new Device();
        var theme = new XuiDesignSystem(new(), device);
        var colors = theme.Colors;
        var changes = 0;
        theme.Changed += _ => changes++;
        theme.Update(new(), device);
        Assert.Equal(0, changes);
        Assert.Same(colors, theme.Colors);
    }

    [Fact]
    public void MutableDevicePreferencesAreComparedByValue()
    {
        var device = new Device();
        var theme = new XuiDesignSystem(new(), device);
        var changes = new List<DesignSystemChange>();
        theme.Changed += changes.Add;
        device.AccessibilityFontScale = (NFloat)1.5;
        theme.Update(new(), device);
        Assert.Equal(DesignSystemChange.Typography, changes[^1]);
        device.ColorScheme = ColorScheme.Dark;
        device.PrefersHighContrast = true;
        theme.Update(new(), device);
        Assert.Equal(DesignSystemChange.Colors, changes[^1]);
        device.PrefersReducedMotion = true;
        theme.Update(new(), device);
        Assert.Equal(DesignSystemChange.Motion, changes[^1]);
        Assert.True(theme.Motion.ReducedMotion);
        theme.Update(new XuiDesignSystemOptions { PrimaryHue = 120, Shape = ShapePreset.Square }, device);
        Assert.Equal(DesignSystemChange.Colors | DesignSystemChange.Shape, changes[^1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedInteractionColorsPreserveTheExistingTones(bool dark)
    {
        var theme = new XuiDesignSystem(new(), new Device { ColorScheme = dark ? ColorScheme.Dark : ColorScheme.Light });
        foreach (var group in new[] { theme.Colors.Primary, theme.Colors.Secondary, theme.Colors.Neutral })
        {
            Assert.Equal(group.Ramp[NFloat.Clamp(group.BackgroundLightness + (dark ? 0.08f : -0.06f), 0, 1)], group.HoverBackground);
            Assert.Equal(group.Ramp[NFloat.Clamp(group.BackgroundLightness + (dark ? 0.15f : -0.12f), 0, 1)], group.PressedBackground);
            Assert.Equal(group.Ramp[NFloat.Clamp(group.BackgroundLightness + (dark ? -0.06f : 0.06f), 0, 1)], group.PressedContainer);
            Assert.Equal(group.Ramp[dark ? 0.85f : 0.48f], group.ToggleHoverBackground);
            Assert.Equal(group.Ramp[dark ? 0.40f : 0.85f], group.SelectedPressedBackground);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SurfacesHaveDistinctTonesAndPickedShadesKeepReadableForegrounds(bool dark)
    {
        var device = new Device { ColorScheme = dark ? ColorScheme.Dark : ColorScheme.Light };
        var theme = new XuiDesignSystem(new(), device);
        var palette = theme.Colors;
        var app = new Oklch(palette.Application.Background).L;
        var card = new Oklch(palette.Surface.Background).L;
        var track = new Oklch(palette.Surface.Container).L;
        Assert.True(NFloat.Abs(app - card) >= 0.05f);
        Assert.True(NFloat.Abs(card - track) >= 0.08f);
        foreach (var hue in new[] { 0, 60, 120, 180, 240, 300 })
        foreach (var tone in new[] { 0f, .1f, .3f, .5f, .7f, .9f, 1f })
        {
            theme.Update(new() { PrimaryHue = hue, Chroma = .25f, PrimaryLightness = tone }, device);
            Assert.Same(palette, theme.Colors);
            static NFloat Luminance(Xui.Core.Canvas.Color c)
            {
                static NFloat Linear(NFloat value) => value <= .04045f ? value / 12.92f : NFloat.Pow((value + .055f) / 1.055f, 2.4f);
                return .2126f * Linear(c.Red) + .7152f * Linear(c.Green) + .0722f * Linear(c.Blue);
            }
            var background = Luminance(palette.Primary.Background);
            var foreground = Luminance(palette.Primary.Foreground);
            var contrast = (NFloat.Max(background, foreground) + .05f) / (NFloat.Min(background, foreground) + .05f);
            Assert.True(contrast >= 4.5f, $"Hue {hue}, tone {tone}: contrast {contrast}");
        }
    }

    private static Dictionary<DesignSystemChange, object> Groups(IDesignSystem theme) => new()
    {
        [DesignSystemChange.Colors] = theme.Colors, [DesignSystemChange.Typography] = theme.Typography,
        [DesignSystemChange.Spacing] = theme.Spacing, [DesignSystemChange.Shape] = theme.Shape,
        [DesignSystemChange.Motion] = theme.Motion,
    };

    private sealed class Device : IDeviceInfo
    {
        public event Action? Changed { add { } remove { } }
        public DevicePlatform Platform => DevicePlatform.iOS;
        public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
        public PointerModel PointerModel => PointerModel.Touch;
        public NFloat AccessibilityFontScale { get; set; } = 1;
        public bool PrefersReducedMotion { get; set; }
        public bool PrefersHighContrast { get; set; }
        public ColorScheme ColorScheme { get; set; }
    }
}
