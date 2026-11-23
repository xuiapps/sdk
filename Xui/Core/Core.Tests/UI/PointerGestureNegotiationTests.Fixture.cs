using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Tests;

public partial class PointerGestureNegotiationTests
{
    private sealed class AllocationObserver : ViewCollection
    {
        public int LastCount;
        public override bool HitTest(Point point) => true;
        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (phase == EventPhase.Tunnel) e.PushPotentialGesture(this, PointerGestures.Drag);
            LastCount = e.PotentialGestures.Length;
        }
    }
}
