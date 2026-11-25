using System;

public static class Program
{
    static (int X, double Y) Move((int X, double Y) value) => (value.X + 20, value.Y + 22);
    static (int X, double Y) Change(ref (int X, double Y) value) { value.X = 9; return value; }
    static (string Name, int Count) Rename((string Name, int Count) value) { value.Name = "copy"; return value; }
    public static int Main()
    {
        var pair = (X: 1, Y: 2.5);
        var moved = Move(pair);
        var (x, y) = moved;
        Console.WriteLine(x);
        Console.WriteLine(y);
        Console.WriteLine(pair.X);
        var nested = (pair, moved, Enabled: true);
        Console.WriteLine(nested.Item2.X);
        var copy = Change(ref pair);
        Console.WriteLine(copy.X);
        var named = (Name: "original", Count: 42);
        var renamed = Rename(named);
        Console.WriteLine(named.Name);
        Console.WriteLine(renamed.Name);
        return named.Count;
    }
}
