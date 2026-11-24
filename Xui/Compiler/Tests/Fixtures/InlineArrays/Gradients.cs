using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public static class Program
{
    private static int Inspect(LinearGradient gradient)
    {
        Console.WriteLine((double)gradient.StartPoint.X);
        Console.WriteLine((double)gradient.EndPoint.X);
        for (int i = 0; i < gradient.GradientStops.Length; i++)
        {
            ref readonly GradientStop stop = ref gradient.GradientStops[i];
            Console.WriteLine((double)stop.Offset);
            Console.WriteLine((double)stop.Color.Red);
            Console.WriteLine((double)stop.Color.Blue);
        }
        return gradient.GradientStops.Length;
    }

    public static int Main()
    {
        ReadOnlySpan<GradientStop> stops = [(0, 0xff0000ffu), (1, 0x0000ffffu)];
        Point start = (0, 0);
        Point end = (100, 0);
        return Inspect(new LinearGradient(start, end, stops));
    }
}
