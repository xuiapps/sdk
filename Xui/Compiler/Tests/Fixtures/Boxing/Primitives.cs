using System;

public static class Program
{
    static object Box<T>(T value) => value;
    static T Read<T>(object value) => (T)value;

    public static int Main()
    {
        int original = 42;
        object boxed = Box(original);
        object alias = boxed;
        original = 99;

        Console.WriteLine(boxed.GetType().Name);
        Console.WriteLine(boxed is int);
        Console.WriteLine(Read<int>(boxed));
        Console.WriteLine(boxed == alias);

        try { Read<uint>(boxed); }
        catch (InvalidCastException) { Console.WriteLine("Wrong type caught"); }

        return Read<int>(boxed);
    }
}
