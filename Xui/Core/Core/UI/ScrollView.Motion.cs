using Xui.Core.Animation;

namespace Xui.Core.UI;

public partial class ScrollView
{
    private nfloat DragOffset(nfloat offset, nfloat delta, nfloat maximum, nfloat viewport) => elastic
        ? ElasticMotion.Resist(ElasticMotion.Unresist(offset, 0, maximum, viewport) + delta, 0, maximum, viewport)
        : nfloat.Clamp(offset + delta, 0, maximum);

    private void QueueReturnToBounds()
    {
        returnX = elastic && (scrollOffsetX < 0 || scrollOffsetX > MaxScrollOffsetX);
        returnY = elastic && (scrollOffsetY < 0 || scrollOffsetY > MaxScrollOffsetY);
        if (returnX || returnY) RequestAnimationFrame();
        if (!elastic) { ClampScrollOffset(); InvalidateScrollOffset(); }
    }

    private bool AdvanceAxis(ref nfloat offset, ref ExponentialDecayCurve? fling, ref nfloat? pendingVelocity,
        ref SpringMotionCurve? spring, ref bool returning, nfloat maximum, nfloat viewport, TimeSpan current)
    {
        if (returning)
        {
            spring = new SpringMotionCurve(current, offset,
                nfloat.Clamp(pendingVelocity ?? 0, -viewport * 4, viewport * 4), nfloat.Clamp(offset, 0, maximum));
            fling = null;
            pendingVelocity = null;
            returning = false;
        }
        else if (pendingVelocity is { } velocity)
        {
            if (nfloat.Abs(velocity) > 50)
                fling = new ExponentialDecayCurve(current, offset, velocity, ExponentialDecayCurve.Normal);
            pendingVelocity = null;
        }

        if (fling is { } curve)
        {
            var value = curve[current];
            var bound = nfloat.Clamp(value, 0, maximum);
            if (value != bound)
            {
                if (elastic)
                {
                    // Start at the actual boundary-crossing time, not the next display
                    // callback. A delayed frame must not restart a stale bounce.
                    var decay = -nfloat.Log(curve.DecayPerSecond);
                    var fraction = nfloat.Clamp(1 - (bound - curve.StartPosition) * decay / curve.InitialVelocity, 0.000000001f, 1);
                    var hit = curve.StartTime + TimeSpan.FromSeconds((double)(-nfloat.Log(fraction) / decay));
                    spring = new SpringMotionCurve(hit, bound,
                        nfloat.Clamp(curve.VelocityAt(hit), -viewport * 8, viewport * 8), bound);
                }
                offset = bound;
                fling = null;
            }
            else
            {
                offset = value;
                if (current >= curve.EndTime) fling = null;
            }
        }

        if (spring is { } rebound)
        {
            var target = nfloat.Clamp(rebound.Target, 0, maximum);
            if (target != rebound.Target)
                spring = rebound = new SpringMotionCurve(current, offset, rebound.VelocityAt(current), target);
            if (rebound.IsComplete(current))
            {
                offset = target;
                spring = null;
            }
            else offset = rebound[current];
        }
        return fling.HasValue || spring.HasValue;
    }
}
