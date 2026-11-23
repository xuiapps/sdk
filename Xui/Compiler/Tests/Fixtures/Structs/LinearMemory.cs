using System;

public struct Point
{
    public double X;
    public double Y;
}

public struct LabelledPoint
{
    public byte Tag;
    public Point Position;
}

public static class Program
{
    public static int Main()
    {
        LabelledPoint original = default;
        original.Tag = 7;
        original.Position.X = 1;
        original.Position.Y = 2;
        var copy = original;
        original.Position.X = 10;
        copy.Position.Y = 20;
        Console.WriteLine(original.Position.X);
        Console.WriteLine(original.Position.Y);
        Console.WriteLine(copy.Position.X);
        Console.WriteLine(copy.Position.Y);
        return copy.Tag;
    }
}
