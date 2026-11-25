using System;

public delegate int Callback(int value);

public class Listener
{
    public int Total;

    public int Add(int value)
    {
        Total += value;
        Console.WriteLine(Total);
        return Total;
    }
}

public static class Program
{
    public static int Main()
    {
        var first = new Listener();
        var second = new Listener { Total = 10 };
        var a = new Callback(first.Add);
        var b = new Callback(second.Add);
        var callbacks = a + b + a;
        Console.WriteLine(callbacks(2));

        // Remove the last matching handler using a fresh delegate object.
        var reduced = callbacks - new Callback(first.Add);
        Console.WriteLine(reduced(3));

        // The original invocation list remains unchanged.
        return callbacks(1);
    }
}
