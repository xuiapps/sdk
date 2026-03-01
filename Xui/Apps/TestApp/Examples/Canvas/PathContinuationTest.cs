using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Examples.Canvas;

/// <summary>
/// Tests path continuation after stroke/fill - known Windows issue where
/// geometry can't be added once closed for draw.
/// </summary>
public class PathContinuationTest : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(White);
        context.FillRect(this.Frame);

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Test 1: Build path, stroke, continue building, stroke again
        // In web canvas, the path persists after stroke
        context.BeginPath();
        context.MoveTo((x + 20, y + 30));
        context.LineTo((x + 100, y + 30));
        context.LineTo((x + 100, y + 80));
        context.SetStroke(Blue);
        context.LineWidth = 4;
        context.Stroke();

        // Continue same path without BeginPath - add more segments
        context.LineTo((x + 20, y + 80));
        context.ClosePath();
        context.SetStroke(Red);
        context.LineWidth = 2;
        context.Stroke();

        // Test 2: Build path, fill, then stroke same path
        context.BeginPath();
        context.MoveTo((x + 140, y + 20));
        context.LineTo((x + 280, y + 20));
        context.LineTo((x + 280, y + 90));
        context.LineTo((x + 140, y + 90));
        context.ClosePath();
        context.SetFill(LightBlue);
        context.Fill();
        // Stroke the same path without BeginPath
        context.SetStroke(DarkBlue);
        context.LineWidth = 3;
        context.Stroke();

        // Test 3: Multiple sub-paths, stroke all at once
        context.BeginPath();
        context.MoveTo((x + 20, y + 120));
        context.LineTo((x + 80, y + 120));
        context.MoveTo((x + 20, y + 140));
        context.LineTo((x + 80, y + 140));
        context.MoveTo((x + 20, y + 160));
        context.LineTo((x + 80, y + 160));
        context.SetStroke(Green);
        context.LineWidth = 3;
        context.Stroke();

        // Test 4: Stroke, change style, stroke again (same path)
        context.BeginPath();
        context.MoveTo((x + 20, y + 200));
        context.LineTo((x + 140, y + 200));
        context.LineTo((x + 140, y + 260));
        context.SetStroke(Orange);
        context.LineWidth = 6;
        context.Stroke();

        // Stroke same path again with different style
        context.SetStroke(Purple);
        context.LineWidth = 2;
        context.Stroke();

        // Test 5: Add to path after stroke, then fill
        context.BeginPath();
        context.MoveTo((x + 180, y + 200));
        context.LineTo((x + 280, y + 200));
        context.LineTo((x + 280, y + 260));
        context.SetStroke(Black);
        context.LineWidth = 3;
        context.Stroke();

        // Continue and close, then fill
        context.LineTo((x + 180, y + 260));
        context.ClosePath();
        context.SetFill(Tomato);
        context.Fill();
    }
}
