using Xui.Core.DI;
using Xui.DevKit.UI.Design;
using Xunit;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Integration.DesignSystem;

public partial class DesignSystemUpdateTest
{
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
}
