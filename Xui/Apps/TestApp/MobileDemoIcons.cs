using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

internal enum MobileDemoIcon
{
    Drawing, Animation, TextMetrics, Basics, Grid, Flexbox, Gestures, TextInput,
    Haptics, Vibration, Recording, Cube, Design, Device, Market,
}

/// <summary>Purple outline symbols in a 32-point area for the home menu.</summary>
internal static class MobileDemoIcons
{
    public static void Draw(IContext context, Point origin, MobileDemoIcon icon)
    {
        context.Save();
        try
        {
            context.Translate(origin);
            context.Translate((2, 2));
            context.Scale((1.4f, 1.4f));
            context.SetStroke(XuiLogo.AccentColor);
            context.LineWidth = 1.6f;
            context.LineCap = LineCap.Round; context.LineJoin = LineJoin.Round;
            switch (icon)
            {
                case MobileDemoIcon.Market:
                    Path(context, [(2, 2), (2, 18), (18, 18)]);
                    Path(context, [(5, 13), (9, 9), (12, 11), (18, 3)]);
                    Path(context, [(13, 3), (18, 3), (18, 8)]);
                    break;
                case MobileDemoIcon.Drawing:
                    Path(context, [(3, 13), (14, 2), (18, 6), (7, 17), (2, 18), (3, 13)]);
                    Line(context, (12, 4), (16, 8));
                    break;
                case MobileDemoIcon.Animation:
                    context.BeginPath(); context.MoveTo((10, 17));
                    context.CurveTo((7, 14), (1, 10), (2, 6));
                    context.CurveTo((3, 1), (8, 2), (10, 5));
                    context.CurveTo((12, 2), (17, 1), (18, 6));
                    context.CurveTo((19, 10), (13, 14), (10, 17));
                    context.ClosePath(); context.Stroke();
                    break;
                case MobileDemoIcon.TextMetrics:
                    Path(context, [(4, 13), (9, 2), (14, 13)]);
                    Line(context, (6, 9), (12, 9));
                    Path(context, [(2, 16), (2, 18), (18, 18), (18, 16)]);
                    break;
                case MobileDemoIcon.Basics:
                    Box(context, (2, 2, 16, 16));
                    Line(context, (2, 7), (18, 7)); Line(context, (8, 7), (8, 18));
                    break;
                case MobileDemoIcon.Grid:
                    Box(context, (2, 2, 6, 6)); Box(context, (12, 2, 6, 6));
                    Box(context, (2, 12, 6, 6)); Box(context, (12, 12, 6, 6));
                    break;
                case MobileDemoIcon.Flexbox:
                    Box(context, (2, 3, 4, 14)); Box(context, (9, 3, 9, 6)); Box(context, (9, 12, 9, 5));
                    break;
                case MobileDemoIcon.Gestures:
                    Path(context, [(4, 12), (8, 16), (8, 6)]);
                    context.BeginPath(); context.MoveTo((8, 6)); context.CurveTo((8, 3), (12, 3), (12, 6));
                    context.LineTo((12, 10)); context.LineTo((16, 11)); context.LineTo((15, 18));
                    context.LineTo((8, 18)); context.Stroke();
                    Path(context, [(2, 6), (2, 2), (6, 2)]);
                    break;
                case MobileDemoIcon.TextInput:
                    Box(context, (1, 4, 18, 12));
                    Line(context, (5, 8), (10, 8)); Line(context, (5, 12), (9, 12));
                    Line(context, (14, 7), (14, 13));
                    break;
                case MobileDemoIcon.Haptics:
                    Circle(context, (10, 10), 3);
                    Line(context, (10, 1), (10, 4)); Line(context, (10, 16), (10, 19));
                    Line(context, (1, 10), (4, 10)); Line(context, (16, 10), (19, 10));
                    Line(context, (3, 3), (5, 5)); Line(context, (15, 15), (17, 17));
                    Line(context, (3, 17), (5, 15)); Line(context, (15, 5), (17, 3));
                    break;
                case MobileDemoIcon.Vibration:
                    Box(context, (6, 3, 8, 14));
                    Path(context, [(3, 4), (1, 7), (3, 10), (1, 13), (3, 16)]);
                    Path(context, [(17, 4), (19, 7), (17, 10), (19, 13), (17, 16)]);
                    break;
                case MobileDemoIcon.Recording:
                    Circle(context, (10, 10), 7);
                    Circle(context, (10, 10), 3);
                    Path(context, [(16, 2), (18, 6), (14, 6)]);
                    break;
                case MobileDemoIcon.Cube:
                    Path(context, [(10, 1), (18, 5), (18, 15), (10, 19), (2, 15), (2, 5), (10, 1)]);
                    Path(context, [(2, 5), (10, 10), (18, 5)]); Line(context, (10, 10), (10, 19));
                    break;
                case MobileDemoIcon.Design:
                    context.BeginPath(); context.MoveTo((10, 2));
                    context.CurveTo((0, 1), (0, 18), (9, 18));
                    context.CurveTo((15, 18), (9, 13), (14, 12));
                    context.CurveTo((22, 12), (19, 2), (10, 2)); context.ClosePath(); context.Stroke();
                    Circle(context, (6, 7), 1); Circle(context, (11, 5), 1); Circle(context, (15, 8), 1);
                    break;
                case MobileDemoIcon.Device:
                    Box(context, (5, 1, 10, 18));
                    Line(context, (8, 4), (12, 4)); Line(context, (9, 16), (11, 16));
                    break;
            }
        }
        finally { context.Restore(); }
    }

    private static void Box(IContext context, Rect rect)
    { context.BeginPath(); context.RoundRect(rect, 1.5f); context.Stroke(); }

    private static void Circle(IContext context, Point center, NFloat radius)
    { context.BeginPath(); context.Arc(center, radius, 0, NFloat.Pi * 2); context.Stroke(); }

    private static void Line(IContext context, Point from, Point to)
    { context.BeginPath(); context.MoveTo(from); context.LineTo(to); context.Stroke(); }

    private static void Path(IContext context, ReadOnlySpan<Point> points)
    {
        context.BeginPath(); context.MoveTo(points[0]);
        foreach (var point in points[1..]) context.LineTo(point);
        context.Stroke();
    }
}
