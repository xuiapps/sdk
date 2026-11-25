using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Tests;

public partial class PointerGestureNegotiationTests
{
    [Fact]
    public void RepeatedDispatchAndOverflowDoNotAllocateGestureStorage()
    {
        var root = new AllocationObserver();
        var leaf = root;
        for (var i = 0; i < 12; i++) { var child = new AllocationObserver(); leaf.Add(child); leaf = child; }
        var router = new EventRouter(root);
        for (var i = 0; i < 256; i++) Send(router);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 256; i++) Send(router);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
        Assert.Equal(8, leaf.LastCount);

        static void Send(EventRouter router)
        {
            var e = new TouchEventRef([new Touch { Index = 0, Phase = TouchPhase.Start, Position = (5, 5), Radius = 1 }]);
            router.Dispatch(ref e);
        }
    }
}
