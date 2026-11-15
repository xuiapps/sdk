using Xui.Core.Math2D;
using Xui.Runtime.IOS.Actual;

namespace Xui.Tests.Component.IOS;

public class SelectionHandleHitTest
{
    private static readonly Rect Start = new(100, 100, 2, 20);
    private static readonly Rect End = new(200, 100, 2, 20);

    [Theory]
    [InlineData(101, 110, 0)] // Stem inside the text field.
    [InlineData(101, 85, 0)] // Leading circle above the stem.
    [InlineData(81, 110, 0)] // Extra finger padding.
    [InlineData(201, 135, 1)] // Trailing circle below the stem.
    [InlineData(221, 110, 1)]
    [InlineData(150, 110, -1)] // Ordinary text taps stay in the canvas.
    [InlineData(101, 65, -1)]
    public void IncludesStemCircleAndPaddingButLeavesOrdinaryTapsAlone(int x, int y, int expected)
        => Assert.Equal(expected, IOSTextSelectionHitTest.Hit((x, y), Start, End, true, true));

    [Fact]
    public void OverlappingTargetsChooseTheNearestVisibleEndpoint()
    {
        Rect end = new(110, 100, 2, 20);
        Assert.Equal(0, IOSTextSelectionHitTest.Hit((102, 110), Start, end, true, true));
        Assert.Equal(1, IOSTextSelectionHitTest.Hit((110, 110), Start, end, true, true));
        Assert.Equal(1, IOSTextSelectionHitTest.Hit((102, 110), Start, end, false, true));
        Assert.Equal(-1, IOSTextSelectionHitTest.Hit((102, 110), Start, end, false, false));
    }
}
