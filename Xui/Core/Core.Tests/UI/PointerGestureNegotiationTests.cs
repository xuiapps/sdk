using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Tests;

public partial class PointerGestureNegotiationTests
{
    [Fact]
    public void CandidatesFollowTunnelOrderAndStorageIsFreshForEveryEvent()
    {
        var outer = new Observer { Advertise = true };
        var inner = new Observer { Advertise = true };
        var leaf = new Observer();
        outer.Add(inner); inner.Add(leaf);
        var router = new EventRouter(outer);
        Send(0, TouchPhase.Start);
        Assert.Equal([outer, inner], leaf.Last.Select(c => c.Handler));
        Assert.All(leaf.Last, candidate => Assert.IsAssignableFrom<IDrag>(candidate.Gesture));

        outer.Advertise = inner.Advertise = false;
        Send(1, TouchPhase.Start);
        Assert.Empty(leaf.Last);
        Send(0, TouchPhase.Move);
        Assert.Empty(leaf.Last);
        Send(0, TouchPhase.Cancel);
        Send(0, TouchPhase.Move);
        Assert.Empty(leaf.Last);

        outer.Advertise = true;
        Send(1, TouchPhase.End);
        Send(1, TouchPhase.Start);
        Assert.Single(leaf.Last);
        Send(1, TouchPhase.End);
        Send(1, TouchPhase.Move);
        Assert.Empty(leaf.Last);

        void Send(int id, TouchPhase phase)
        {
            var e = new TouchEventRef([new Touch { Index = id, Phase = phase, Position = (5, 5), Radius = 1 }]);
            router.Dispatch(ref e);
        }
    }

    [Fact]
    public void OverflowKeepsTheEightNearestHandlersInOrder()
    {
        var handlers = Enumerable.Range(0, 12).Select(_ => new Observer { Advertise = true }).ToArray();
        for (var i = 1; i < handlers.Length; i++) handlers[i - 1].Add(handlers[i]);
        var leaf = new Observer();
        handlers[^1].Add(leaf);
        var router = new EventRouter(handlers[0]);
        var e = new TouchEventRef([new Touch { Index = 0, Phase = TouchPhase.Start, Position = (5, 5), Radius = 1 }]);
        router.Dispatch(ref e);
        Assert.Equal(handlers.Skip(4), leaf.Last.Select(candidate => candidate.Handler));
        Assert.All(handlers, handler => Assert.Equal(handlers.Skip(4), handler.Last.Select(candidate => candidate.Handler)));
    }

    private sealed class Observer : ViewCollection
    {
        public bool Advertise;
        public PointerGestureCandidate[] Last = [];
        public override bool HitTest(Point point) => true;
        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (Advertise && phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
            {
                e.PushPotentialGesture(this, PointerGestures.Drag);
            }
            if (phase == EventPhase.Bubble)
            {
                e.PushPotentialGesture(this, PointerGestures.Tap); // Too late; must be ignored.
                Last = e.PotentialGestures.ToArray();
            }
        }
    }
    [Fact]
    public void RepeatedDispatchAndOverflowKeepEightCandidates()
    {
        var root = new AllocationObserver();
        var leaf = root;
        for (var i = 0; i < 12; i++) { var child = new AllocationObserver(); leaf.Add(child); leaf = child; }
        var router = new EventRouter(root);
        for (var i = 0; i < 256; i++) Send(router);
        for (var i = 0; i < 256; i++) Send(router);
        Assert.Equal(8, leaf.LastCount);

        static void Send(EventRouter router)
        {
            var e = new TouchEventRef([new Touch { Index = 0, Phase = TouchPhase.Start, Position = (5, 5), Radius = 1 }]);
            router.Dispatch(ref e);
        }
    }
}
