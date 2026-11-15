using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Invisible touch padding around native selection stems and lollipops.</summary>
internal static class IOSTextSelectionHitTest
{
    internal static int Hit(Point point, Rect start, Rect end, bool startVisible, bool endVisible)
    {
        bool nearStart = startVisible && Bounds(start).Contains(point);
        bool nearEnd = endVisible && Bounds(end).Contains(point);
        if (!nearStart) return nearEnd ? 1 : -1;
        if (!nearEnd) return 0;
        // Short selections may overlap. Both ends must remain reachable.
        return DistanceSquared(start, point) <= DistanceSquared(end, point) ? 0 : 1;
    }

    private static Rect Bounds(Rect caret) => new(caret.X - 24, caret.Y - 24, caret.Width + 48, caret.Height + 48);

    private static NFloat DistanceSquared(Rect caret, Point point)
    {
        NFloat dx = point.X - (caret.X + caret.Width / 2);
        NFloat dy = point.Y - NFloat.Clamp(point.Y, caret.Y, caret.Bottom);
        return dx * dx + dy * dy;
    }
}
