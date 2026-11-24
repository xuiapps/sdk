using System;

public delegate int Read();
public delegate void Add(int amount);

public class Counter
{
    public Read Read;
    public Add Add;
}

public static class Program
{
    private static Counter Create(int start)
    {
        int count = start;
        return new Counter
        {
            Read = () => count,
            Add = amount => count += amount
        };
    }

    public static int Main()
    {
        var first = Create(10);
        var second = Create(100);
        first.Add(2);
        first.Add(3);
        second.Add(7);
        Console.WriteLine(first.Read());
        Console.WriteLine(second.Read());
        return first.Read() + second.Read();
    }
}
