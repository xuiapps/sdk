using Xui.Core.Canvas;
using Xui.Core.Canvas.Filter;

namespace Xui.Core.Tests.Canvas;

public class BrightnessTests
{
    [Fact]
    public void DefaultStateIsIdentity()
    {
        var color = new Color(0.8f, 0.4f, 0.2f, 0.3f);
        var state = default(FilterState);
        Assert.Equal(FilterKind.None, state.Kind);
        Assert.Equal((nfloat)1, state.Amount);
        Assert.Equal(color, state.Apply(color));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0.5, 0.375, 0.25, 0.125)]
    [InlineData(1, 0.75, 0.5, 0.25)]
    [InlineData(2, 1, 1, 0.5)]
    public void MultipliesRgbAndPreservesAlpha(double amount, double red, double green, double blue)
    {
        var state = new FilterState(new Brightness((nfloat)amount));
        var actual = state.Apply(new Color(0.75f, 0.5f, 0.25f, 0.25f));
        Assert.Equal(FilterKind.Brightness, state.Kind);
        Assert.Equal(new Color((nfloat)red, (nfloat)green, (nfloat)blue, (nfloat)0.25), actual);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void RejectsInvalidAmounts(double amount) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Brightness((nfloat)amount));
}
