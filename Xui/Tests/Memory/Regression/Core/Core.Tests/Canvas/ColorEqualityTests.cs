using Xui.Core.Canvas;

namespace Xui.Core.Tests.Canvas;

public partial class ColorEqualityTests
{
    [Fact]
    public void GenericEqualityDoesNotBoxColors()
    {
        var comparer = EqualityComparer<Color>.Default;
        var a = new Color(0.1f, 0.2f, 0.3f, 1);
        var b = a;
        for (var i = 0; i < 100; i++) comparer.Equals(a, b);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var equal = true;
        for (var i = 0; i < 1000; i++) equal &= comparer.Equals(a, b);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(equal);
        Assert.Equal(0, allocated);
    }
}
