using System;

public interface IContext { int Read(); }
public class Context : IContext
{
    public int Value;
    public int Read() => Value;
}
public struct Point { public int X; public int Y; }
public ref struct Event
{
    public Point Position;
    public string Name;
    public IContext Context;
}
public static class Program
{
    static void Dispatch(ref Event e)
    {
        e.Position.X += e.Context.Read();
        e.Name = "handled";
        Forward(ref e);
    }
    static void Forward(ref Event e) { e.Position.Y = 2; }
    public static int Main()
    {
        Event e = default;
        e.Name = "original";
        e.Context = new Context { Value = 40 };
        Event copy = e;
        Dispatch(ref e);
        Console.WriteLine(e.Name);
        Console.WriteLine(copy.Name);
        Console.WriteLine(copy.Context == e.Context);
        return e.Position.X + e.Position.Y;
    }
}
