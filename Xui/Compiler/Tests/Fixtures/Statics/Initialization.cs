using System;

public struct Point
{
    public int X, Y;
}

public static class Settings
{
    public static readonly Point Origin = new Point { X = 20, Y = 22 };
    public static string Name = "Xui";

    static Settings()
    {
        Console.WriteLine("Settings initialized");
    }
}

public static class Program
{
    public static int Main()
    {
        Console.WriteLine("Main");
        Console.WriteLine(Settings.Name);
        Point copy = Settings.Origin;
        copy.X = 99;
        Console.WriteLine(Settings.Origin.X);
        return Settings.Origin.X + Settings.Origin.Y;
    }
}
