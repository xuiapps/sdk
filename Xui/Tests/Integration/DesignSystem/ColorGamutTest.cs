using Xui.Core.Canvas;
using Xui.DevKit.UI.Design;

namespace Xui.Tests.Integration.DesignSystem;

public class ColorGamutTest
{
    [Theory]
    [InlineData(0.15, 30)]
    [InlineData(0.56, 90)]
    [InlineData(0.70, 240)]
    [InlineData(0.95, 330)]
    public void GamutBoundaryPreservesLightnessAndHue(double lightness, double hue)
    {
        var chroma = Oklch.MaxSrgbChroma((nfloat)lightness, (nfloat)hue);
        Assert.InRange((double)chroma, 0.001, 0.37);
        var roundTrip = new Oklch(new Oklch((nfloat)lightness, chroma, (nfloat)hue).ToColor());
        Assert.Equal(lightness, (double)roundTrip.L, precision: 4);
        Assert.Equal(hue, (double)roundTrip.H, precision: 2);
        Assert.Equal((double)chroma, (double)roundTrip.C, precision: 4);
        // Above the boundary, conversion necessarily clips at least one channel.
        var clipped = new Oklch(new Oklch((nfloat)lightness, chroma + 0.01f, (nfloat)hue).ToColor());
        Assert.True(nfloat.Abs(clipped.C - chroma - 0.01f) > 0.001f);
    }

    [Theory]
    [InlineData(0xFF0000FFu)]
    [InlineData(0x00FF00FFu)]
    [InlineData(0x0000FFFFu)]
    public void SaturatedSrgbPrimariesRoundTrip(uint rgba)
    {
        var original = new Color(rgba);
        var restored = new Oklch(original).ToColor();
        Assert.Equal((double)original.Red, (double)restored.Red, precision: 4);
        Assert.Equal((double)original.Green, (double)restored.Green, precision: 4);
        Assert.Equal((double)original.Blue, (double)restored.Blue, precision: 4);
    }
}
