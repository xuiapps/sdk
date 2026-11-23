using System;

public struct Point
{
    public int X;
    public int Y;
}

public class Unit
{
    string name;
    Point position;

    public Unit(string name, int x, int y)
    {
        this.name = name;
        position.X = x;
        position.Y = y;
    }

    public void MoveTo(int x, int y)
    {
        position.X = x;
        position.Y = y;
    }

    public static int Main()
    {
        var unit = new Unit("Scout", 1, 2);
        var alias = unit;
        alias.MoveTo(4, 6);

        var copy = unit.position;
        copy.X = 20;
        Console.WriteLine(unit.name);
        Console.WriteLine(unit.position.X);
        Console.WriteLine(unit.position.Y);
        Console.WriteLine(copy.X);

        unit.position = copy;
        copy.X = 99;
        Console.WriteLine(alias.position.X);
        Console.WriteLine(unit == alias);
        return 0;
    }
}
