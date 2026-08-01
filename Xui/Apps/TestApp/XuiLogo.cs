using System.Runtime.InteropServices;
using Xui.Core.Canvas;

namespace Xui.Apps.TestApp;

/// <summary>The Xui mark, drawn entirely by the canvas renderer.</summary>
internal sealed class XuiLogo
{
    public static readonly XuiLogo Instance = new();

    public void Render(IContext context)
    {
        context.Save();

        context.BeginPath();
        context.MoveTo((0, 32));
        context.CurveTo((0, 4.16f), (4.16f, 0), (32, 0));
        context.CurveTo((64 - 4.16f, 0), (64, 4.16f), (64, 32));
        context.CurveTo((64, 64 - 4.16f), (64 - 4.16f, 64), (32, 64));
        context.CurveTo((4.16f, 64), (0, 64 - 4.16f), (0, 32));
        context.ClosePath();
        context.Clip();

        context.SetFill(new LinearGradient(
            start: (10, 5),
            end: (54, 59),
            gradient: [new(0, 0xD642CDFF), new(1, 0x8A05FFFF)]));
        context.FillRect((0, 0, 64, 64));

        context.SetFill(new LinearGradient(
            start: (0, 20),
            end: (64, 44),
            gradient: [new(0, 0xE8BEEDFF), new(1, 0xE4CFE5FF)]));

        context.BeginPath();
        context.MoveTo((5, 0));
        context.LineTo((15, 22));
        context.LineTo((25, 0));
        context.ClosePath();
        context.Fill();

        context.BeginPath();
        context.MoveTo((5, 64));
        context.LineTo((15, 42));
        context.LineTo((25, 64));
        context.ClosePath();
        context.Fill();

        context.BeginPath();
        context.MoveTo((0, 10));
        context.LineTo((10, 32));
        context.LineTo((0, 54));
        context.ClosePath();
        context.Fill();

        context.SetStroke(Colors.White);
        context.LineWidth = 5;
        context.BeginPath();
        context.MoveTo((25, 24));
        context.LineTo((25, 35));
        context.CurveTo((25, 45.5f), (32, 45.5f));
        context.CurveTo((39, 45.5f), (39, 35));
        context.LineTo((39, 24));
        context.Stroke();

        context.MoveTo((49, 24));
        context.LineTo((49, 47));
        context.Stroke();

        context.SetFill(Colors.White);
        context.BeginPath();
        context.Ellipse((49, 18), 3, 3, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
        context.Fill();

        context.Restore();
    }
}
