using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Tessellate;

namespace Xui.Tests.Component;

public class PathTessellatorTest
{
    [Theory]
    [InlineData(FillRule.NonZero, true, 84)]
    [InlineData(FillRule.NonZero, false, 100)]
    [InlineData(FillRule.EvenOdd, true, 84)]
    [InlineData(FillRule.EvenOdd, false, 84)]
    public void HorizontalEdgesAreFiniteAndContourWindingPreservesHoles(FillRule rule, bool reversed, int expectedArea)
    {
        var path = new Path2D();
        path.Rect((0, 0, 10, 10));
        if (reversed)
        {
            path.MoveTo((3, 3)); path.LineTo((3, 7)); path.LineTo((7, 7)); path.LineTo((7, 3)); path.ClosePath();
        }
        else path.Rect((3, 3, 4, 4));
        var triangles = PathTesselator.Fill(path, rule).Polygons;
        Assert.NotEmpty(triangles);
        double area = 0;
        foreach (var t in triangles)
        {
            Assert.All(new[] { t.A, t.B, t.C }, p => Assert.True(nfloat.IsFinite(p.X) && nfloat.IsFinite(p.Y)));
            area += Math.Abs((double)Vector.Cross(t.B - t.A, t.C - t.A)) / 2;
        }
        Assert.Equal(expectedArea, area, 6);
    }
}
