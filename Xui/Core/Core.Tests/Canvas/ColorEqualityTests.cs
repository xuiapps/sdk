using Xui.Core.Canvas;

namespace Xui.Core.Tests.Canvas;

public class ColorEqualityTests
{
    [Fact]
    public void EqualityIncludesEveryChannel()
    {
        var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
        Assert.Equal(color, new Color(0.1f, 0.2f, 0.3f, 0.4f));
        Assert.Equal(color.GetHashCode(), new Color(0.1f, 0.2f, 0.3f, 0.4f).GetHashCode());
        Assert.NotEqual(color, new Color(0.5f, 0.2f, 0.3f, 0.4f));
        Assert.NotEqual(color, new Color(0.1f, 0.5f, 0.3f, 0.4f));
        Assert.NotEqual(color, new Color(0.1f, 0.2f, 0.5f, 0.4f));
        Assert.NotEqual(color, new Color(0.1f, 0.2f, 0.3f, 0.5f));
        Assert.False(color.Equals(null));
        Assert.False(color.Equals("color"));
    }

    [Fact]
    public void EqualityPreservesFloatingPointValueSemantics()
    {
        var a = new Color(nfloat.NaN, 0, nfloat.PositiveInfinity, 1);
        var b = new Color(nfloat.NaN, -0.0f, nfloat.PositiveInfinity, 1);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GenericEqualityPreservesColorValues()
    {
        var comparer = EqualityComparer<Color>.Default;
        var a = new Color(0.1f, 0.2f, 0.3f, 1);
        var b = a;
        for (var i = 0; i < 100; i++) comparer.Equals(a, b);
        var equal = true;
        for (var i = 0; i < 1000; i++) equal &= comparer.Equals(a, b);
        Assert.True(equal);
    }
}
