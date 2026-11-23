using System;

public struct Point { public int X; public int Y; }

public class Unit
{
    public string Name;
    public Point Position;
    public Unit(string name) { Name = name; }
    public virtual int Power() => 1;
    public virtual string Kind() => "unit";
}

public class Scout : Unit
{
    public int Bonus;
    public Scout(string name, int bonus) : base(name) { Bonus = bonus; }
    public override int Power() => base.Power() + Bonus;
    public override string Kind() => "scout";
}

public class Veteran : Scout
{
    public Veteran(string name, int bonus) : base(name, bonus) { }
    public override int Power() => base.Power() + 10;
}

public class Medic : Unit
{
    public Medic(string name) : base(name) { }
}

public static class Program
{
    static int Inspect(Unit unit)
    {
        Console.WriteLine(unit.Name);
        Console.WriteLine(unit.Kind());
        Console.WriteLine(unit.Power());
        Console.WriteLine(unit.Position.X);
        Console.WriteLine(unit is Scout);
        var scout = unit as Scout;
        if (scout != null) Console.WriteLine(scout.Bonus);
        Console.WriteLine(unit is Medic);
        Console.WriteLine((unit as Medic) == null);
        return unit.Power();
    }

    public static int Main()
    {
        var unit = new Veteran("Ada", 3);
        unit.Position.X = 4;
        unit.Position.Y = 6;
        return Inspect(unit);
    }
}
