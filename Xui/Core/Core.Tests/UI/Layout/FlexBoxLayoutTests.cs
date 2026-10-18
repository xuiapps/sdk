using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;

namespace Xui.Core.UI.Tests.Layout;

public class FlexBoxLayoutTests
{
    private static FixedView Box(int width, int height) => new() { Size = (width, height) };

    [Fact]
    public void GrowingItemsShareRemainingSpace()
    {
        var a = Box(40, 20); a[FlexBox.Grow] = 1;
        var b = Box(40, 20); b[FlexBox.Grow] = 3;
        var flex = new FlexBox { ColumnGap = 10, Content = [a, b] };
        flex.Arrange((0, 0, 200, 50), null!);
        Assert.Equal(new Rect(0, 0, 67.5f, 50), a.Frame);
        Assert.Equal(new Rect(77.5f, 0, 122.5f, 50), b.Frame);
    }

    [Fact]
    public void ShrinkRedistributesAfterMinimumIsReached()
    {
        var a = Box(100, 20); a.MinimumWidth = 80;
        var b = Box(100, 20);
        var flex = new FlexBox { Content = [a, b] };
        flex.Arrange((0, 0, 120, 30), null!);
        Assert.Equal((nfloat)80, a.Frame.Width);
        Assert.Equal((nfloat)40, b.Frame.Width);
        Assert.Equal((nfloat)120, b.Frame.Right);
    }

    [Fact]
    public void GrowRedistributesAfterMaximumIsReached()
    {
        var a = Box(20, 20); a.MaximumWidth = 40; a[FlexBox.Grow] = 1;
        var b = Box(20, 20); b[FlexBox.Grow] = 1;
        var flex = new FlexBox { Content = [a, b] };
        flex.Arrange((0, 0, 200, 30), null!);
        Assert.Equal((nfloat)40, a.Frame.Width);
        Assert.Equal((nfloat)160, b.Frame.Width);
    }

    [Fact]
    public void WrappingRespondsToAvailableWidthAndPreservesGaps()
    {
        var a = Box(60, 20); var b = Box(60, 30); var c = Box(60, 20);
        var flex = new FlexBox { FlexWrap = FlexBox.Wrap.Wrap,
            FlexAlignContent = FlexBox.AlignContent.FlexStart, ColumnGap = 10, RowGap = 8, Content = [a, b, c] };
        Assert.Equal(new Size(130, 58), flex.Measure((140, 200), null!));
        flex.Arrange((10, 20, 140, 200), null!);
        Assert.Equal(new Rect(10, 20, 60, 30), a.Frame);
        Assert.Equal(new Rect(80, 20, 60, 30), b.Frame);
        Assert.Equal(new Rect(10, 58, 60, 20), c.Frame);
        flex.Arrange((0, 0, 200, 200), null!);
        Assert.Equal((nfloat)0, c.Frame.Y);
        Assert.Equal((nfloat)140, c.Frame.X);
    }

    [Fact]
    public void OrderColumnReverseAndCrossAxisAlignment()
    {
        var a = Box(20, 30); a[FlexBox.Order] = 1;
        var b = Box(40, 30);
        var flex = new FlexBox { FlexDirection = FlexBox.Direction.ColumnReverse,
            FlexAlignItems = FlexBox.AlignItems.Center, RowGap = 10, Content = [a, b] };
        flex.Arrange((0, 0, 100, 100), null!);
        Assert.Equal(new Rect(30, 70, 40, 30), b.Frame);
        Assert.Equal(new Rect(40, 30, 20, 30), a.Frame);
    }

    [Fact]
    public void JustificationChangeInvalidatesPreviouslyArrangedGeometry()
    {
        var a = Box(20, 20); var b = Box(20, 20);
        var flex = new FlexBox { Content = [a, b] };
        flex.Arrange((0, 0, 100, 20), null!);
        Assert.Equal((nfloat)20, b.Frame.X);
        flex.FlexJustifyContent = FlexBox.JustifyContent.SpaceBetween;
        flex.Arrange((0, 0, 100, 20), null!);
        Assert.Equal((nfloat)80, b.Frame.X);
    }

    [Fact]
    public void FractionalGrowCanLeaveSpaceForJustification()
    {
        var a = Box(20, 20); a[FlexBox.Grow] = 0.25f;
        var b = Box(20, 20); b[FlexBox.Grow] = 0.25f;
        var flex = new FlexBox { FlexJustifyContent = FlexBox.JustifyContent.SpaceBetween, Content = [a, b] };
        flex.Arrange((0, 0, 100, 20), null!);
        Assert.Equal((nfloat)35, a.Frame.Width);
        Assert.Equal((nfloat)65, b.Frame.X);
    }

    [Fact]
    public void CrossAxisMaximumAndMarginsArePreserved()
    {
        var a = Box(20, 20); a.Margin = 5; a.MaximumHeight = 30;
        var flex = new FlexBox { Content = [a] };
        flex.Arrange((0, 0, 100, 100), null!);
        Assert.Equal(new Rect(5, 5, 20, 30), a.Frame);
    }

    [Fact]
    public void WrapReversePlacesFirstLineAtCrossEnd()
    {
        var a = Box(60, 20); var b = Box(60, 20);
        var flex = new FlexBox { FlexWrap = FlexBox.Wrap.WrapReverse,
            FlexAlignContent = FlexBox.AlignContent.FlexStart, RowGap = 10, Content = [a, b] };
        flex.Arrange((0, 0, 100, 100), null!);
        Assert.Equal((nfloat)80, a.Frame.Y);
        Assert.Equal((nfloat)50, b.Frame.Y);
    }
}
