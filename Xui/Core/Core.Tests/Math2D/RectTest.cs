namespace Xui.Core.Math2D;

public class RectTest
{
    [Fact]
    public void InequalityIsTheComplementOfEquality()
    {
        var rect = new Rect(1, 2, 30, 40);
        Assert.False(rect != new Rect(1, 2, 30, 40));
        Assert.True(rect != new Rect(1, 2, 30, 41));
        Assert.True(rect != new Rect(2, 2, 30, 40));
        Assert.True(rect != new Rect(1, 3, 30, 40));
        Assert.True(rect != new Rect(1, 2, 31, 40));
    }

    [Fact]
    public void Rect_Ctor_PointAndSize()
    {
        var rect = new Rect(new Point(2, 3), new Size(10, 5));
        Assert.Equal(2, rect.Left);
        Assert.Equal(3, rect.Top);
        Assert.Equal(12, rect.Right);
        Assert.Equal(8, rect.Bottom);
    }

    [Fact]
    public void Rect_Ctor_VectorAndSize()
    {
        var rect = new Rect(new Vector(2, 3), new Size(10, 5));
        Assert.Equal(2, rect.Left);
        Assert.Equal(3, rect.Top);
        Assert.Equal(12, rect.Right);
        Assert.Equal(8, rect.Bottom);
    }

    [Fact]
    public void Rect_CornerProps()
    {
        var rect = new Rect(5, 10, 20, 15);
        Assert.Equal((5, 10), rect.TopLeft);
        Assert.Equal((25, 10), rect.TopRight);
        Assert.Equal((25, 25), rect.BottomRight);
        Assert.Equal((5, 25), rect.BottomLeft);
    }

    [Fact]
    public void Rect_Size()
    {
        var rect = new Rect(5, 10, 20, 15);
        Assert.Equal(20, rect.Width);
        Assert.Equal(15, rect.Height);
        rect.Size = (30, 25);
        Assert.Equal((5, 10), rect.TopLeft);
        Assert.Equal((35, 35), rect.BottomRight);
    }

    [Fact]
    public void Rect_Inset()
    {
        Assert.Equal((7, 12, 16, 11), new Rect(5, 10, 20, 15).Inset(2));
    }

    [Fact]
    public void Rect_Expand()
    {
        Assert.Equal((3, 8, 24, 19), new Rect(5, 10, 20, 15).Expand(2));
        Assert.Equal((2, 6, 26, 23), new Rect(5, 10, 20, 15).Expand(3, 4));
    }

    [Fact]
    public void Rect_Contains()
    {
        var rect = new Rect(-20, -10, 30, 10);
        Assert.True(rect.Contains((-5, -5)));
        Assert.True(rect.Contains((5, 0)));
        Assert.False(rect.Contains((-30, 0)));
        Assert.False(rect.Contains((-10, 5)));
    }
}
