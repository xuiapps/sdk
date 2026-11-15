using System.Collections.Generic;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Middleware.Emulator.UI.Phone;

/// <summary>Draws active mouse contacts as the touch indicators used by the emulator.</summary>
internal sealed class TouchOverlay
{
    private readonly Dictionary<int, Point> activeTouches = [];

    public void Begin(int pointerId, Point position) => activeTouches[pointerId] = position;

    public void Move(int pointerId, Point position) => activeTouches[pointerId] = position;

    public void End(int pointerId) => activeTouches.Remove(pointerId);

    public void Render(IContext context, NFloat scale)
    {
        foreach (var touch in activeTouches.Values)
        {
            var radius = 15f * scale;
            context.BeginPath();
            context.Ellipse(touch, radius, radius, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
            context.SetFill(0x66888888);
            context.Fill();

            context.BeginPath();
            context.Ellipse(touch, radius, radius, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
            context.LineWidth = 3f * scale;
            context.SetStroke(0x88AAAAAA);
            context.Stroke();
        }
    }
}
