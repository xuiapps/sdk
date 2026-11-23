using System.Runtime.InteropServices;
using Xui.Runtime.IOS.Actual;

namespace Xui.Tests.Component.IOS;

public partial class IOSDrawingStateTest
{
    [Fact]
    public void RepeatedWarmFramesWithNestedDashesAllocateNothing()
    {
        var state = new IOSDrawingState();
        void Frame()
        {
            state.Reset();
            state.SetLineDash([2, 4]);
            var saved = state.Save();
            state.SetLineDash([1, 2, 3]);
            state.LineDashOffset = 3;
            state.SetLineDash([]);
            state.Restore(saved);
            state.LineDashOffset = 5;
        }
        for (int i = 0; i < 10; i++) Frame();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) Frame();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
