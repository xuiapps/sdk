using System;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        Point point = (0, 0);
        Vector step = (3, 4);
        point += step;
        Size size = (120, 80);
        Rect bounds = (10, 20, 120, 80);
        Frame margin = (5, 10);
        Rect expanded = bounds + margin;
        Console.WriteLine((double)point.X);
        Console.WriteLine((double)point.Y);
        Console.WriteLine((double)size.Width);
        Console.WriteLine((double)expanded.X);
        Console.WriteLine((double)expanded.Y);
        Console.WriteLine((double)expanded.Width);
        Console.WriteLine((double)expanded.Height);
        return 0;
    }
}
